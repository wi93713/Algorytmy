using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
		private const double GRADUATION		= 0.8;				// skalowanie na canvasie

        private bool m_bDataTypeCity			= false;           // FALSE = POINTS, TRUE = MIASTA
		private List<City> m_lstCity			= null;             // lista wczytanych miast/punktów
		private List<City> m_lstUnvisitedCities = null;				// lista nnieodwiedzonych Miast - potrzebne do drugiej strasy.
		private ArrayList[] m_arrIncidentList	= null;           // lista incydencji 
		private int m_iMaxDistance = 7600;                      // maksymalna długość ścieżki
		private int m_iStartCity = 1;							// numer miasta początkowego (wybrane z kontrolki)
		
		//********* Deklaracje zmiennych tylko dla danych punktówych******************
		private int m_iPointsCount = 0;                 // liczba punktów
		private double[,] m_arrDistances;				// tablica odległości NxN


        public MainWindow()
        {
            InitializeComponent();
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

		private void Oblicz_Click(object sender, RoutedEventArgs e)
		{

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
                City bestCity = GetBestCity(currentCity, lstUnvisdCit , distance, profit); // Znajdz najlepsze miasto profit/distance
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
		// Wyszukujemy najlepsze miasto dzieląc profit przez dystans (wykład 4 p.10)
		private City GetBestCity(City current, List<City> unvistedNodes, double distance, double profit)
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
						newRoute = optSwap(bestRoute, i, k);
						newDist = CalcDistance(newRoute);
						if (newDist < bestDist)
						{
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
		// https://www.technical-recipes.com/2017/applying-the-2-opt-algorithm-to-travelling-salesman-problems-in-c-wpf/
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
            foreach (var n in lstCities)
            {
                dProfit = dProfit + n.m_dProfit;
            }
            return dProfit;
        }

		private Path Insert(Path path, double max)
        {
            int n = path.m_lstVisitedCities.Count;
            List<City> tempPath = new List<City>();
            List<City> currentPath = new List<City>(path.m_lstVisitedCities);
            List<City> unvisited = new List<City>(path.m_lstUnvisitedCities);
            double profit = path.m_dSumProfit;
            double tempProfit = 0;
            double tempDistance = 0;
            bool improve = true;
            while (improve)
            {
                improve = false;
                for (int v = 0; v < unvisited.Count-1; v++)
                {
                    for (int i = 1; i < n - 1; i++)
                    {
                        tempPath = ConstructNewPath(i, unvisited.ElementAt(v), currentPath);
                        tempProfit = CalcProfit(tempPath);
                        tempDistance = CalcDistance(tempPath);
                        if ((tempProfit > profit) && (tempDistance <= max))
                        {
                            currentPath = tempPath;
                            profit = tempProfit;
                            improve = true;
                            unvisited.RemoveAt(v);

                        }
                    }
                }      
            }
			return new Path(currentPath, CalcDistance(currentPath), profit, unvisited);
        }

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
	}
}
