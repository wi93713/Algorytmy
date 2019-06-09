using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Maps.MapControl.WPF;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Algorytmy2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
	
    public partial class MainWindow : Window
    {
		private const double GRADUATION		= 0.6;				// skalowanie na canvasie

        private bool m_bDataTypeCity			= false;           // FALSE = POINTS, TRUE = MIASTA
		private List<City> m_lstCity			= null;             // lista wczytanych miast/punktów
		private List<City> m_lstUnvisitedCities = null;				// lista nnieodwiedzonych Miast - potrzebne do drugiej strasy.
		private ArrayList[] m_arrIncidentList	= null;           // lista incydencji 
        private int m_GetRandomPath = 0;                            // ustaw randomowa sciezke co x elementow
		Random random = new Random();
        private int m_iMaxDistance = 7600;                      // maksymalna długość ścieżki
		private int m_iStartCity = 1;							// numer miasta początkowego (wybrane z kontrolki)
		
		//********* Deklaracje zmiennych tylko dla danych punktówych******************
		private int m_iPointsCount = 0;                 // liczba punktów
		private double[,] m_arrDistances;               // tablica odległości NxN

        //public class ViewModel
        //{
        //    public Heurystyka m_Heurystyka { get; set; } = new Heurystyka();
        //    public PunktStartowy m_PunktStartowy { get; set; } = new PunktStartowy();
        //}

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new Heurystyka();
        }

		///////////////////////////////////////////////////////////////////////////////
		/// Event Otwórz plik
		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			dlg.InitialDirectory = "c:\\";
			dlg.FileName = "test"; // Default file name
            dlg.Filter = "txt files (*.txt)|*.txt";
            dlg.FilterIndex = 2;
            dlg.RestoreDirectory = true;
			if( dlg.ShowDialog() == true)
			{
				LoadData(dlg.FileName);
				// TODO? MOZE JAKIEŚ USTAWIENIA
			}
		}

		///////////////////////////////////////////////////////////////////////////////
		/// Wczytanie danych
		private void LoadData(string sFileName)
		{
			if( m_lstCity != null ) {
				m_lstCity.Clear();
			}
			m_lstCity = new List<City>();
			System.IO.StreamReader sr = new System.IO.StreamReader(sFileName);  

			// TODO rozbić na dwa typy danych. 
			if( !m_bDataTypeCity )
			{
				LoadPoints(sr);
				CalcDistances();
				DrawPointsOnCanvas();
			}
			else
			{
				// todo
				LoadCities();
			}
		}

		
		///////////////////////////////////////////////////////////////////////////////
		/// Wczytanie punktów
		private void LoadPoints(System.IO.StreamReader sr)
		{
			sr.DiscardBufferedData(); // przejdz do początku pliku
			m_iPointsCount = Convert.ToInt32(sr.ReadLine());
			string line = null;
			for (int i = 0; i < m_iPointsCount; i++)
			{
				line = sr.ReadLine();
				if (line != null)
				{
					var arrLine = line.Split(' ');
					City point = new City
					{
						m_iNumber = i,
						X =  Double.Parse(arrLine[0], NumberStyles.Float, CultureInfo.InvariantCulture),
						Y =	Double.Parse(arrLine[1], NumberStyles.Float, CultureInfo.InvariantCulture),
						m_dProfit = Convert.ToDouble(arrLine[2]),
						m_bWasVisited = false
					};
					point.m_sName = "Numer Punktu: " + i + "\nProfit: " + point.m_dProfit +" X: " + point.X + " Y: " + point.Y; 
					m_lstCity.Add(point);
				}
				else return;
			}

		}
		///////////////////////////////////////////////////////////////////////////////
		/// Obliczenie wszystkich par odległości 
		private void CalcDistances()
		{
			m_arrDistances = new double[m_iPointsCount, m_iPointsCount];
			for (int i = 0; i < m_iPointsCount; i++)
			{
				for (int j = 0; j < m_iPointsCount; j++)
				{
					double x = m_lstCity[i].X - m_lstCity[j].X;
					double y = m_lstCity[i].Y - m_lstCity[j].Y;
					m_arrDistances[i, j] = Convert.ToDouble(Math.Floor(Math.Sqrt(x * x + y * y)));
				}
			}
		}
		private void DrawPointsOnCanvas()
		{
			double maxProfit = m_lstCity.Max(x => x.m_dProfit);
            canvas.Height = m_lstCity.Max(x => x.X) + 2; //TODO  Zmienić rozmiar Canvsa?
            canvas.Width = m_lstCity.Max(x => x.Y) + 2;
			SolidColorBrush mySolidColorBrush = new SolidColorBrush();
			mySolidColorBrush.Color = Color.FromArgb(255, 1, 1, 1);
            foreach (var point in m_lstCity)
            {
                Ellipse ellipse = new Ellipse() { Width = point.m_dProfit/maxProfit + 5, Height = point.m_dProfit/maxProfit + 5, Stroke = new SolidColorBrush(Colors.Black) };
				ellipse.ToolTip = point.m_sName;
				ellipse.Fill = mySolidColorBrush;
                Canvas.SetLeft(ellipse, point.X * GRADUATION); // TODO DODAĆ SKALOWANIE
                Canvas.SetTop(ellipse, point.Y * GRADUATION);
                canvas.Children.Add(ellipse);
            }
		}

		///////////////////////////////////////////////////////////////////////////////
		/// Wczytanie miast 
		private void LoadCities()
		{
			throw new NotImplementedException();
		}

        private void SaveData(object sender, RoutedEventArgs e)
        {

        }

        private void DrawLines(List<City> path, SolidColorBrush color)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Line line = new Line();
                line.Stroke = color;

                line.X1 = path.ElementAt(i).X * GRADUATION;
                line.X2 = path.ElementAt(i + 1).X * GRADUATION;
                line.Y1 = path.ElementAt(i).Y * GRADUATION;
                line.Y2 = path.ElementAt(i + 1).Y * GRADUATION;

                line.StrokeThickness = 2;
                canvas.Children.Add(line);
            }
            Line line1 = new Line();
            line1.Stroke = color;

            line1.X1 = path.ElementAt(0).X * GRADUATION;
            line1.X2 = path.ElementAt(path.Count - 1).X * GRADUATION;
            line1.Y1 = path.ElementAt(0).Y * GRADUATION;
            line1.Y2 = path.ElementAt(path.Count - 1).Y * GRADUATION;

            line1.StrokeThickness = 2;
            canvas.Children.Add(line1);

        }

		private void Oblicz_Click(object sender, RoutedEventArgs e)
		{
            //string ComboHeurystyka = (string)heurystyka.SelectedItem.ToString();
            //string ComboPunktStartowy = (string)punktStartowy.Text;

           // string w_distance = textBox.Text; // dystans pobrany z okna
            m_iMaxDistance = Int32.Parse(textBox.Text);
            Path newPath = MetodaGreedy();
             Path modifiedPath = LocalSearch(newPath);
            Path modifiedPath2  = GreedyLocalSearch2(500);
			if (modifiedPath2.m_dSumProfit > modifiedPath.m_dSumProfit) 
				DrawLines(modifiedPath2.m_lstVisitedCities, Brushes.Blue);
			else	
				DrawLines(modifiedPath.m_lstVisitedCities, Brushes.Blue);
            //Console.WriteLine(ComboHeurystyka);
            //Console.WriteLine(ComboPunktStartowy);
            //Console.WriteLine(w_distance);


        }


        private Path GreedyLocalSearch2(int numberOfRouts)
        {
            // conctruct N routes then find best one and take localsearch on it
            List<Path> routs = new List<Path>();
            for (int i = 0; i < numberOfRouts; i++)
            {
                routs.Add(MetodaGreedyRand());
            }
            Path best = routs.ElementAt(0);

            foreach (Path r in routs)
            {
                if (r.m_dSumProfit > best.m_dSumProfit)
                {
                    best = r;
                }
            }
            best = LocalSearch(best);
            return best;
        }

        ///////////////////////////////////////////////////////////////////////////////
        // Zachłanna zwraca
        private Path MetodaGreedy()
		{ 
			double distance = 0;
            double profit = 0;
			int iDistMax = m_iMaxDistance;
            List<City> path = new List<City>();					// route
            List<City> lstUnvisdCit = new List<City>(m_lstCity);	// nieodwiedzone miasta
            City currentCity;
            City startCity = new City();
            startCity = m_lstCity.ElementAt(m_iStartCity);
            path.Add(startCity);   //add start point to route
            lstUnvisdCit.Remove(startCity);
            currentCity = startCity;

            while (distance < m_iMaxDistance)
            {
                City bestCity = GetBestCity(currentCity, lstUnvisdCit); // Znajdz najlepsze miasto profit/distance
                if (CheckDistance(distance, iDistMax, currentCity, bestCity, startCity)) // sprawdz czy mozną wrócić do miasta początkowego
                {
                        distance = distance + m_arrDistances[currentCity.m_iNumber, bestCity.m_iNumber];
                        profit = profit + bestCity.m_dProfit;
                        path.Add(bestCity);
                        lstUnvisdCit.Remove(bestCity);
                        currentCity = bestCity;
                }
                else
                {
                    break;
                }
            }
            path.Add(path.ElementAt(0));
            distance = distance + m_arrDistances[path.ElementAt(path.Count() - 2).m_iNumber, path.ElementAt(path.Count() - 1).m_iNumber];
			m_lstUnvisitedCities = lstUnvisdCit;

            return new Path(path, distance, profit, lstUnvisdCit);
		}

        ///////////////////////////////////////////////////////////////////////////////
        // Zachłanna zwraca
        private Path MetodaGreedyRand()
        {
            double distance = 0;
            double profit = 0;
            int iDistMax = m_iMaxDistance;
            List<City> path = new List<City>();					// route
            List<City> lstUnvisdCit = new List<City>(m_lstCity);	// nieodwiedzone miasta
            City currentCity = m_lstCity.ElementAt(m_iStartCity);
            path.Add(currentCity);   //add start point to route
            lstUnvisdCit.Remove(currentCity);

            while (distance < m_iMaxDistance)
            {
                City bestCity = GetBestCityRandom(currentCity, lstUnvisdCit, distance); // Znajdz najlepsze miasto profit/distance
				if (bestCity != null)
				{
					distance = distance + m_arrDistances[currentCity.m_iNumber, bestCity.m_iNumber];
					profit = profit + bestCity.m_dProfit;
					path.Add(bestCity);
					lstUnvisdCit.Remove(bestCity);
					currentCity = bestCity;
					m_GetRandomPath++;
				}
				else
					break;
            }
            path.Add(path.ElementAt(0));
            distance = distance + m_arrDistances[path.ElementAt(path.Count() - 2).m_iNumber, path.ElementAt(path.Count() - 1).m_iNumber];
            m_lstUnvisitedCities = lstUnvisdCit;

            return new Path(path, distance, profit, lstUnvisdCit);
        }

        ///////////////////////////////////////////////////////////////////////////////
        // Wyszukujemy najlepsze miasto dzieląc profit przez dystans (wykład 4 p.10)
        private City GetBestCity(City current, List<City> unvistedNodes)
        {
            City best = new City();
            double bestProfit = 0;
            foreach (var n in unvistedNodes)
            {
                if (current != n)
                {
                    if (m_arrDistances[current.m_iNumber, n.m_iNumber] != double.MaxValue)
                    {
                        if ((n.m_dProfit) / (m_arrDistances[current.m_iNumber, n.m_iNumber]) > bestProfit)
                        {
                            best = n;
                            bestProfit = (n.m_dProfit) / (m_arrDistances[current.m_iNumber, n.m_iNumber]);
                        }
                    }
                }
            }

            return best;
        }

        ///////////////////////////////////////////////////////////////////////////////
        // Wyszukujemy najlepsze miasto + co 18 raz random
        private City GetBestCityRandom(City current, List<City> lstUnvisitedCities, double dCurrentDistance)
        {
			City best = null ;
            double bestProfit = 0;
            foreach (var n in lstUnvisitedCities)
            {
                if (current != n)
                {
					if (m_GetRandomPath == 15)
					{
						best = GetShortestWayNode(current, lstUnvisitedCities, dCurrentDistance);
						m_GetRandomPath = 0;
						return best;
					}
					if (m_arrDistances[current.m_iNumber, n.m_iNumber] != double.MaxValue)
                    {
                        if ((n.m_dProfit) / (m_arrDistances[current.m_iNumber, n.m_iNumber]) > bestProfit && 
							( m_arrDistances[current.m_iNumber, n.m_iNumber] + m_arrDistances[m_iStartCity, n.m_iNumber] + dCurrentDistance  <= m_iMaxDistance) )
                        {
                            best = n;
                            bestProfit = (n.m_dProfit) / (m_arrDistances[current.m_iNumber, n.m_iNumber]);                     
                        }
                    }
                }
            }

            return best;
        }

        private City GetShortestWayNode(City current, List<City> unvisitedCities, double dCurrentDistance)
        {
            double bestDist = double.MaxValue;
			City bestCity = null;
			List<City> lstRandom = new List<City>();
            foreach (var n in unvisitedCities)
            {
				if (current != n)
				{
					if (n.m_dProfit == 0)
						continue;
					if( bestDist > m_arrDistances[current.m_iNumber, n.m_iNumber])
					if (m_iMaxDistance >= dCurrentDistance + m_arrDistances[current.m_iNumber, n.m_iNumber] + m_arrDistances[n.m_iNumber, m_iStartCity])
					{
						bestDist = m_arrDistances[current.m_iNumber, n.m_iNumber];
						lstRandom.Add(n);
					}
				}
            }
			if (lstRandom.Count > 0)
			{
				int iSize = lstRandom.Count() - 1;
				bestCity = lstRandom.ElementAt(random.Next(0, iSize));
			}
			return bestCity;
        }
        ///////////////////////////////////////////////////////////////////////////////
        // Sprawdzamy czy mozemy dojść do puntu startowego
        private bool CheckDistance(double currentDistance, double maxDistance, City current, City next, City start)
        {
            if (m_arrDistances[current.m_iNumber, next.m_iNumber] + currentDistance < maxDistance)
            {
                double tempDistance = m_arrDistances[current.m_iNumber, next.m_iNumber] + currentDistance;
                if (tempDistance + m_arrDistances[next.m_iNumber, start.m_iNumber] <= maxDistance)
                {
                    return true;
                }
            }
            return false;
        }
		///////////////////////////////////////////////////////////////////////////////
		//  Local search
		private Path LocalSearch(Path path)
		{
			Path bestPath = path;
			Path tmp = null;
			bool bImprove = true;
			while (bImprove)
			{
				bImprove = false;
				tmp = TwoOpt(bestPath);
				if (tmp.m_dSumDistance <= bestPath.m_dSumDistance)
				{
					tmp = Insert(tmp, m_iMaxDistance);
					if (tmp.m_dSumProfit > bestPath.m_dSumProfit)
					{
						bImprove = true;
						bestPath = tmp;
					}
				}
			}
			return bestPath;
		}

		///////////////////////////////////////////////////////////////////////////////
		// 2OPT wykład 3 
		private Path TwoOpt(Path path)
        {
            List<City> newRoute = new List<City>();
            List<City> bestRoute = new List<City>(path.m_lstVisitedCities);
            int n = path.m_lstVisitedCities.Count;
            double newDist = 0;
            double bestDist = path.m_dSumDistance;
            double bestProfit = path.m_dSumProfit;
            bool bImprove = true;
			while (bImprove)
			{
				bImprove = false;
				for (int i = 1; i < n - 1; i++)
				{
					for (int k = i + 1; k < n - 1; k++)
					{
                        newDist = optCheckSwapNeeded(bestRoute, bestDist, i, k);
						//newDist = CalcDistance(newRoute);
						if (newDist < bestDist)
						{
                            newRoute = optSwap(bestRoute, i, k);
							bestRoute = newRoute;
							bestDist = newDist;
							bImprove = true;
						}
					}
				}
			}
			Path newPath = new Path(bestRoute, bestDist, CalcProfit(bestRoute), path.m_lstUnvisitedCities);
            return newPath;

        }

        private double optCheckSwapNeeded(List<City> route, double actualDistance, int i, int k)
        {
            double dDistance = actualDistance;
            dDistance -= m_arrDistances[route.ElementAt(i - 1).m_iNumber, route.ElementAt(i).m_iNumber];
            dDistance -= m_arrDistances[route.ElementAt(k).m_iNumber, route.ElementAt(k + 1).m_iNumber];
            dDistance += m_arrDistances[route.ElementAt(i).m_iNumber, route.ElementAt(k + 1).m_iNumber];
            dDistance += m_arrDistances[route.ElementAt(k).m_iNumber, route.ElementAt(i - 1).m_iNumber];
            return dDistance;
        }
        private double optCheckInsertNeeded(List<City> route, double actualDistance, int v, int n)
        {
            double dDistance = actualDistance;
            dDistance -= m_arrDistances[route.ElementAt(n-1).m_iNumber, route.ElementAt(n).m_iNumber];
            dDistance += m_arrDistances[route.ElementAt(n-1).m_iNumber, v];
            dDistance += m_arrDistances[v, route.ElementAt(n).m_iNumber];
            return dDistance;
        }
		private List<City> optSwap(List<City> route, int i, int k)
		{
			List<City> newRoute = new List<City>();
			List<City> temp = new List<City>();
			List<City> order = new List<City>();


			//1.take route[0] to route[i - 1] and add them in order to new_route
			for (int a = 0; a < i; a++)
			{
				newRoute.Add(route.ElementAt(a));
			}
			//2.take route[i] to route[k] and add them in reverse order to new_route
			for (int b = i; b < k + 1; b++)
			{
				temp.Add(route.ElementAt(b));
			}
			temp.Reverse();
			newRoute.AddRange(temp);
			//3.take route[k + 1] to end and add them in order to new_route
			for (int c = k + 1; c < route.Count; c++)
			{
				newRoute.Add(route.ElementAt(c));
			}

			return newRoute;
		}

		private double CalcDistance(List<City> lstCities)
		{
			double dist = 0;
			for (int i = 0; i <= lstCities.Count - 2; i++)
			{
				dist = dist + m_arrDistances[lstCities.ElementAt(i).m_iNumber, lstCities.ElementAt(i + 1).m_iNumber];
			}
			return dist;
		}

		private double CalcProfit(List<City> lstCities)
        {
            double dProfit = 0;
			double dLast = 0;
            foreach (var n in lstCities)
            {
                dProfit = dProfit + n.m_dProfit;
				dLast = n.m_dProfit;
            }
            return dProfit - dLast;
        }

		private Path Insert(Path path, double max)
        {
            int n = path.m_lstVisitedCities.Count;
            List<City> tempPath = new List<City>();
           
            List<City> unvisited = new List<City>(path.m_lstUnvisitedCities);
			//List<City> notMatch = new List<City>();

            List<City> bestPath = new List<City>(path.m_lstVisitedCities);
            double bestProfit   = path.m_dSumProfit;
            double bestDistance = path.m_dSumDistance;
            double newDistance  = bestDistance;

            bool improve = true;
            while (improve)
            {
                improve = false;
                for (int v = 0; v < unvisited.Count-1; v++)
                {
					//notMatch = unvisited;
					if(unvisited.ElementAt(v).m_dProfit == 0)
						continue;
                    for (int i = 1; i < n - 1; i++)
                    {
						if (v > unvisited.Count - 1 )
						{
							improve = false;
							v = unvisited.Count;
							break;
						}
						//City bestCity = GetBestCity(bestPath.ElementAt(i), notMatch);
                        newDistance = optCheckInsertNeeded(bestPath, bestDistance, unvisited.ElementAt(v).m_iNumber, i); 
                        if(newDistance <= max)
                        {
                            tempPath = ConstructNewPath(i, unvisited.ElementAt(v), bestPath);
                            bestProfit = bestProfit + unvisited.ElementAt(v).m_dProfit;
                            bestDistance = newDistance;
                            bestPath = tempPath;      
                            improve = true;
                            unvisited.RemoveAt(v);
                        }
						else
						{
							//notMatch.Remove(bestCity);
						}
                    }
                }      
            }
            return new Path(bestPath, bestDistance, bestProfit, unvisited);
        }
		//private Path Insert(Path path, double max)
  //      {
  //          int n = path.m_lstVisitedCities.Count;
  //          List<City> tempPath = new List<City>();
           
  //          List<City> unvisited = new List<City>(path.m_lstUnvisitedCities);

  //          List<City> bestPath = new List<City>(path.m_lstVisitedCities);
  //          double bestProfit   = path.m_dSumProfit;
  //          double bestDistance = path.m_dSumDistance;
  //          double newDistance  = 0;
  //          bool improve = true;
  //          while (improve)
  //          {
  //              improve = false;
  //              for (int v = 0; v < unvisited.Count-1; v++)
  //              {
		//			if(unvisited.ElementAt(v).m_dProfit == 0) { 
		//				continue;
		//			}
					
  //                  for (int i = 1; i < n - 1; i++)
  //                  {
  //                      //City bestCity = GetBestCity(bestPath.ElementAt(i), unvisited);
		//				if(v > (unvisited.Count - 1) ) {
		//					improve = false;
		//					break;
		//				}
  //                      newDistance = optCheckInsertNeeded(bestPath, bestDistance, unvisited.ElementAt(v).m_iNumber, i); 
  //                      if(newDistance <= max)
  //                      {
  //                          tempPath = ConstructNewPath(i, unvisited.ElementAt(v), bestPath);
  //                          bestProfit = bestProfit + unvisited.ElementAt(v).m_dProfit;
  //                          bestDistance = newDistance;
  //                          bestPath = tempPath;      
  //                          improve = true;
  //                          unvisited.RemoveAt(v);
  //                      }
  //                  }
  //              }      
  //          }
  //          return new Path(bestPath, bestDistance, bestProfit, unvisited);
  //      }
		private List<City> ConstructNewPath(int insertIdx, City insertNode, List<City> currentPath)
        {
            List<City> newPath = new List<City>();

            //add first half of current path
            for(int i=0;i<insertIdx;i++)
            {
                newPath.Add(currentPath.ElementAt(i));
            }
            //add node to list on insert index
            newPath.Add(insertNode);
            for(int i=insertIdx;i<currentPath.Count;i++)
            {
                newPath.Add(currentPath.ElementAt(i));
            }
            return newPath;
        }

        public class Heurystyka
        {
            public Heurystyka()
            {
                Heurystyki = new List<string>();
                Heurystyki.Add("pierwsza");
                Heurystyki.Add("druga");
                Heurystyki.Add("trzecia");

            }
            public IList<string> Heurystyki { get; set; }

        }

        public class PunktStartowy
        {
            public PunktStartowy()
            {
                Startowy = new List<string>();
                Startowy.Add("jedssen");
                Startowy.Add("dssa");
            }
            public IList<string> Startowy { get; set; }
        }

        private void heurystyka_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}