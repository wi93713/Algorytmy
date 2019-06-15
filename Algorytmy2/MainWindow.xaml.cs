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
using MoreLinq;

namespace Algorytmy2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private const double GRADUATION = 0.6;				// skalowanie na canvasie

        private bool m_bDataTypeCity = true;           // FALSE = POINTS, TRUE = MIASTA
        private bool m_bFirstPath = true;               // TRUE = Rysuj pierwszą trase , FALSE = rysuj kolejną
        private List<City> m_lstCity = null;             // lista wczytanych miast/punktów
        private List<City> m_lstUnvisitedCities = null;             // lista nnieodwiedzonych Miast - potrzebne do drugiej strasy.
        private ArrayList[] m_arrIncidentList = null;             // lista incydencji 
        private Path m_PreviousPath = null;
        private int m_GetRandomPath = 0;                            // ustaw randomowa sciezke co x elementow
        Random m_random = new Random();
        private int m_iMaxDistance = 7600;                      // maksymalna długość ścieżki
        private int m_iStartCity = 1;                           // numer miasta początkowego (wybrane z kontrolki)
		private int m_Strength = 1;

        //********* Deklaracje zmiennych dla danych z mapy Polski*********************
        private int m_iCitiesCount = 0;
        private double[,] m_iCitiesDistance;

        //********* Deklaracje zmiennych tylko dla danych punktówych******************
        private int m_iPointsCount = 0;                 // liczba punktów
        private double[,] m_arrDistances;               // tablica odległości NxN

        private const string m_sHeurystyka1 = "Metoda Greed Random + LocalSearch";
        private const string m_sHeurystyka2 = "Metoda ILS";
        private static int m_nHeurystyka = 0;
        public enum Heurystyka
        {
            GRLS = 1,
            ILS = 2,
        }
        public MainWindow()
        {
            InitializeComponent();
            DisableAllButtons();

            cboxHeurystyka.Items.Add(m_sHeurystyka1);
            cboxHeurystyka.Items.Add(m_sHeurystyka2);
        }

        private void DisableAllButtons()
        {
            cboxHeurystyka.IsEnabled = false;
            cboxPunktStartowy.IsEnabled = false;
            txboxMaxDistance.IsEnabled = false;
            btnOblicz.IsEnabled = false;

        }
        private void EnableAllButtons()
        {
            cboxPunktStartowy.SelectedIndex = 0;
            cboxHeurystyka.SelectedIndex = 0;
            cboxHeurystyka.IsEnabled = true;
            cboxPunktStartowy.IsEnabled = true;
            txboxMaxDistance.IsEnabled = true;
            btnOblicz.IsEnabled = true;

        }
		#region WczytywanieDanych
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
            if (dlg.ShowDialog() == true)
            {
                LoadData(dlg.FileName);
                EnableAllButtons();

            }
        }

        ///////////////////////////////////////////////////////////////////////////////
        /// Wczytanie danych
        private void LoadData(string sFileName)
        {
            if (m_lstCity != null)
            {
                m_lstCity.Clear();
            }
            m_lstCity = new List<City>();
            System.IO.StreamReader tr = new System.IO.StreamReader(sFileName);
            System.IO.StreamReader sr = new System.IO.StreamReader(sFileName);
            DataTypeCity(tr);
            tr.Close();

            if (!m_bDataTypeCity)
            {
                LoadPoints(sr);
                CalcDistances();
                DrawPointsOnCanvas();
            }
            else
            {
                // todo
                LoadCities(sr);
                CalcDistances();
                m_arrIncidentList = DijkstraAlgotytm.setIncidenceList(m_iCitiesCount, m_iCitiesDistance, m_lstCity);
            }
        }

		 ///////////////////////////////////////////////////////////////////////////////
        /// Jaki typ danych
		private void DataTypeCity(StreamReader sr)
        {
            sr.DiscardBufferedData(); // przejdz do początku pliku SPRYTNE XD
            int Count = Convert.ToInt32(sr.ReadLine());
            if (Count > 202)
            {
                m_bDataTypeCity = false;
            }
            else
                m_bDataTypeCity = true;
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
                        X = Double.Parse(arrLine[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                        Y = Double.Parse(arrLine[1], NumberStyles.Float, CultureInfo.InvariantCulture),
                        m_dProfit = Convert.ToDouble(arrLine[2]),
                        m_bWasVisited = false
                    };
                    point.m_sName = "Numer Punktu: " + i + "\nProfit: " + point.m_dProfit + " X: " + point.X + " Y: " + point.Y;
                    m_lstCity.Add(point);
                    cboxPunktStartowy.Items.Add(i);
                }
                else return;
            }
        }

		///////////////////////////////////////////////////////////////////////////////
        /// Wczytanie miast 
        private void LoadCities(StreamReader sr)
        {
            sr.DiscardBufferedData(); // przejdz do początku pliku
            m_iCitiesCount = Convert.ToInt32(sr.ReadLine());
            string line = null;
            for (int i = 0; i < m_iCitiesCount; i++)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    var arrLine = line.Split(' ');
                    City city = new City
                    {
                        m_iNumber = i,
                        X = Double.Parse(arrLine[3], NumberStyles.Float, CultureInfo.InvariantCulture),
                        Y = Double.Parse(arrLine[4], NumberStyles.Float, CultureInfo.InvariantCulture),
                        m_dProfit = Convert.ToDouble(arrLine[2]),
                        m_bWasVisited = false,
                        m_sName = Convert.ToString(arrLine[1])

                    };
                    m_lstCity.Add(city);
                    cboxPunktStartowy.Items.Add(i);
                }
                else return;
            }
        }

		///////////////////////////////////////////////////////////////////////////////
		///Dodanie punktów na canvasie
		private void DrawPointsOnCanvas()
		{
			double maxProfit = m_lstCity.Max(x => x.m_dProfit);
			canvas.Height = m_lstCity.Max(x => x.X) + 2;
			canvas.Width = m_lstCity.Max(x => x.Y) + 2;
			SolidColorBrush mySolidColorBrush = new SolidColorBrush();
			mySolidColorBrush.Color = Color.FromArgb(255, 1, 1, 1);
			foreach (var point in m_lstCity)
			{
				Ellipse ellipse = new Ellipse() { Width = point.m_dProfit / maxProfit + 5, Height = point.m_dProfit / maxProfit + 5, Stroke = new SolidColorBrush(Colors.Black) };
				ellipse.ToolTip = point.m_sName;
				ellipse.Fill = mySolidColorBrush;
				Canvas.SetLeft(ellipse, point.X * GRADUATION);
				Canvas.SetTop(ellipse, point.Y * GRADUATION);
				canvas.Children.Add(ellipse);
			}
		}
		#endregion

		///////////////////////////////////////////////////////////////////////////////
		/// Obliczenie wszystkich par odległości 
		private void CalcDistances()
        {
            m_iCitiesDistance = new double[m_iCitiesCount, m_iCitiesCount];
            m_arrDistances = new double[m_iPointsCount, m_iPointsCount];
            for (int i = 0; i < m_iPointsCount; i++)
            {
                for (int j = 0; j < m_iPointsCount; j++)
                {
                    double x = m_lstCity[i].X - m_lstCity[j].X;
                    double y = m_lstCity[i].Y - m_lstCity[j].Y;
                    m_arrDistances[i, j] = Convert.ToDouble(Math.Floor(Math.Sqrt(x * x + y * y)));
                    //m_iCitiesDistance[i, j] = Convert.ToDouble(Math.Floor(Math.Sqrt(x * x + y * y)));
                }
            }
        }

		///////////////////////////////////////////////////////////////////////////////
		/// Rysowanie Trasy
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


		private void SaveData(object sender, RoutedEventArgs e)
        {

        }

		private void heurystyka_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string text = (sender as ComboBox).SelectedItem as string;
			if (text.CompareTo(m_sHeurystyka1) == 0)
			{
				labHeurystykaAddon.Visibility = Visibility.Visible;
				txboxHeurystykaAddon.Visibility = Visibility.Visible;
				m_nHeurystyka = (int)Heurystyka.GRLS;
			}
			else
			{
				labHeurystykaAddon.Visibility = Visibility.Hidden;
				txboxHeurystykaAddon.Visibility = Visibility.Hidden;
				m_nHeurystyka = (int)Heurystyka.ILS;
			}
		}

       

        private void ShowPath(Path path, Path path2)
        {
            List<City> cities = path.m_lstVisitedCities;

            string displayedPath = "Trasa 1: ";
            foreach (City city in cities)
            {
                displayedPath += city.m_iNumber.ToString() + ' ';
            }
            string displayedInfo = "Trasa 1 : Profit : " + path.m_dSumProfit + "\nDystans : " + path.m_dSumDistance;
            if (path2 != null)
            {
                List<City> cities2 = path2.m_lstVisitedCities;
                displayedPath += "\nTrasa 2: ";
                foreach (City city in cities2)
                {
                    displayedPath += city.m_iNumber.ToString() + ' ';
                }
                displayedInfo += "\nTrasa2 : Profit : " + path2.m_dSumProfit + "\nDystans : " + path2.m_dSumDistance;
            }
            displayedCitiesText.Text = displayedPath;
            displayedOtherText.Text = displayedInfo;
            displayedOtherText.Visibility = Visibility.Visible;
            displayedCitiesText.Visibility = Visibility.Visible;
            if (path2 != null)
            {
                CheckTwoPatch(path.m_lstVisitedCities, path2.m_lstVisitedCities);
                double dDistance1 = 0, dDistance2 = 0, dProfit1 = 0, dProfit2 = 0;
                CheckLenAndProfit(path.m_lstVisitedCities, path2.m_lstVisitedCities, out dProfit1, out dProfit2, out dDistance1, out dDistance2);
                if (path.m_dSumDistance == dDistance1 && path2.m_dSumDistance == dDistance2)
                {
                    // ok
                }
                else
                {
					MessageBox.Show("Hmmmm....");
                }
            }
        }

        private bool CheckTwoPatch(List<City> lstVisitedCities1, List<City> lstVisitedCities2)
        {
            foreach (City city in lstVisitedCities1)
            {
                if (city.m_iNumber == m_iStartCity)
                    continue;
                foreach (City city2 in lstVisitedCities2)
                {
                    if (city2.m_iNumber == m_iStartCity)
                        continue;
                    if (city.m_iNumber == city2.m_iNumber)
                        return true;
                }
            }
            return false;
        }
        private void CheckLenAndProfit(List<City> lstVisitedCities1, List<City> lstVisitedCities2, out double dProfit1, out double dProfit2, out double dDistance1, out double dDistance2)
        {
            dDistance1 = 0;
            dDistance2 = 0;
            dProfit1 = 0;
            dProfit2 = 0;
            for (int i = 0; i < lstVisitedCities1.Count - 1; i++)
            {
                dDistance1 += m_arrDistances[lstVisitedCities1.ElementAt(i).m_iNumber, lstVisitedCities1.ElementAt(i + 1).m_iNumber];
                dProfit1 += lstVisitedCities1.ElementAt(i).m_dProfit;
            }
            for (int i = 0; i < lstVisitedCities2.Count - 1; i++)
            {
                dDistance2 += m_arrDistances[lstVisitedCities2.ElementAt(i).m_iNumber, lstVisitedCities2.ElementAt(i + 1).m_iNumber];
                dProfit2 += lstVisitedCities2.ElementAt(i).m_dProfit;
            }
        }

        private void Oblicz_Click(object sender, RoutedEventArgs e)
        {
            if (!m_bDataTypeCity)
            {
                m_iMaxDistance = Int32.Parse(txboxMaxDistance.Text);
                m_iStartCity = Int32.Parse(cboxPunktStartowy.SelectedItem.ToString());
                Path path = null;
                if (m_nHeurystyka == (int)Heurystyka.GRLS)
                {
                    path = GreedyRandomLocalSearch();
                }
                else
                {
					path = IteratedLocaSearchMethod();
                }
                if (m_bFirstPath)
                {
                    m_PreviousPath = path;
                    ShowPath(path, null);
                    DrawLines(path.m_lstVisitedCities, Brushes.Blue);
                    m_bFirstPath = false;
                }
                else
                {
                    DrawLines(path.m_lstVisitedCities, Brushes.Red);
                    ShowPath(m_PreviousPath, path);
                }

            }
            else
            {

            }

        }

        private Path GreedyRandomLocalSearch()
        {
            Path modifiedPath = LocalSearch(MetodaGreedy());
            int iValue = Int32.Parse(txboxHeurystykaAddon.Text);
            if (iValue > 10000) iValue = 10000;

            Path modifiedPath2 = GreedyRandomLocalSearch2(iValue);
            m_lstUnvisitedCities = modifiedPath2.m_lstUnvisitedCities;
            m_lstUnvisitedCities.Insert(0, m_lstCity.ElementAt(m_iStartCity));
            m_lstCity = m_lstUnvisitedCities;
            return modifiedPath2;
        }
		private Path IteratedLocaSearchMethod()
        {
			int n = 2;
            if (n > 500) n = 500;
			int m = 250;
            Path modifiedPath2 = IteratedLocaSearch(n, m);
            m_lstUnvisitedCities = modifiedPath2.m_lstUnvisitedCities;
            m_lstUnvisitedCities.Insert(0, m_lstCity.ElementAt(m_iStartCity));
            m_lstCity = m_lstUnvisitedCities;
            return modifiedPath2;
        }

		private Path IteratedLocaSearch(int n, int m)
		{
			Path bestPath = new Path();
			bestPath = MetodaGreedy();
			while(n > 0)
			{
				m_Strength = 1;
				int noProgres1 = 0, noProgres2 = 0;
				Path t = MetodaGreedyRand();
				//t.m_lstUnvisitedCities = t.m_lstUnvisitedCities.OrderByDescending(x => x.m_dProfit).ToList();
				Path tlok = t;
				for(int i = 0; i<= m; i++)
				{
					t = new Path(t);
					t = Disturb(t);
					t = LocalSearch(t);
					if(t.m_dSumProfit > bestPath.m_dSumProfit && t.m_dSumDistance <= m_iMaxDistance)
					{
						m_Strength = 1;
						noProgres1 = 0;
						bestPath = new Path(t);
					}
					else
					{ 
						noProgres1++;
						if (noProgres1 == 40)
							m_Strength++;
						if( noProgres1 > 150)
						{
							m_Strength = 6;
						}
						//if (noProgres1 == 80)
						//	m_Strength++;
						//if (m_Strength > 4)
						//	m_Strength = 4;
						//if (noProgres1 > 100)
						//	m_Strength = 6;
						//if (noProgres1 > 150)
						//	m_Strength = 8;
					}
					if(t.m_dSumProfit > tlok.m_dSumProfit && t.m_dSumDistance <= m_iMaxDistance)
					{
						m_Strength--;
						if (m_Strength == 0)
							m_Strength = 1;
						noProgres2 = 0;
						tlok = new Path(t);
					}
					else
					{ 
						noProgres2++;
						if (noProgres2 == 10 || noProgres2 == 20 || noProgres2 == 40)
							m_Strength++;
						//if (noProgres2 > 100 && m_Strength < 6 )
						//	m_Strength = 6;
					}
				}
				--n;
			}
			return bestPath;
		}

		private Path Disturb(Path t)
		{
			for (int i = 0; i < (m_Strength > 2 ? 2 : 1); i++)
			{
				double min = t.m_lstVisitedCities.Min(x => x.m_dProfit);
				City tmpCity = t.m_lstVisitedCities.First(x => x.m_dProfit == min);
				if (tmpCity.m_iNumber == m_iStartCity)
				{
					break;
				}
				int Index = t.m_lstVisitedCities.IndexOf(tmpCity);
				City NextCity = t.m_lstVisitedCities.ElementAt(Index + 1);
				City PrevCity = t.m_lstVisitedCities.ElementAt(Index - 1);

				t.m_dSumDistance = t.m_dSumDistance - m_arrDistances[tmpCity.m_iNumber, NextCity.m_iNumber] - m_arrDistances[tmpCity.m_iNumber, PrevCity.m_iNumber]
					+ m_arrDistances[NextCity.m_iNumber, PrevCity.m_iNumber];
				t.m_dSumProfit -= tmpCity.m_dProfit;
				t.m_lstVisitedCities.Remove(tmpCity);
				t.m_lstUnvisitedCities.Add(tmpCity);
			}
			t.m_lstUnvisitedCities = t.m_lstUnvisitedCities.OrderByDescending(x => x.m_dProfit).ToList();
			List<City> RemovedCities = new List<City>();
			if (m_Strength > 2)
			{
				for (int i = 0; i < m_Strength - 2; i++)
				{
					int max = t.m_lstVisitedCities.Count - 2 > i * 50 + 50 ? i * 50 + 50 - i : t.m_lstVisitedCities.Count - 2 - i;
					int iRemove = m_random.Next(1, max);
					City tmpCity = t.m_lstVisitedCities.ElementAt(iRemove);
					t.m_dSumProfit -= t.m_lstVisitedCities.ElementAt(iRemove).m_dProfit;
					t.m_dSumDistance -= m_arrDistances[t.m_lstVisitedCities.ElementAt(iRemove - 1).m_iNumber, t.m_lstVisitedCities.ElementAt(iRemove).m_iNumber];
					t.m_dSumDistance -= m_arrDistances[t.m_lstVisitedCities.ElementAt(iRemove).m_iNumber, t.m_lstVisitedCities.ElementAt(iRemove + 1).m_iNumber];
					t.m_lstVisitedCities.Remove(tmpCity);

					if (t.m_lstUnvisitedCities.Count > 0)
					{
						int iInsert = m_random.Next(0, t.m_lstUnvisitedCities.Count - 1);
						t.m_lstVisitedCities.Insert(iRemove, t.m_lstUnvisitedCities.ElementAt(iInsert));
						t.m_dSumProfit += t.m_lstVisitedCities.ElementAt(iRemove).m_dProfit;
						t.m_dSumDistance += m_arrDistances[t.m_lstVisitedCities.ElementAt(iRemove - 1).m_iNumber, t.m_lstVisitedCities.ElementAt(iRemove).m_iNumber];
						t.m_dSumDistance += m_arrDistances[t.m_lstVisitedCities.ElementAt(iRemove).m_iNumber, t.m_lstVisitedCities.ElementAt(iRemove + 1).m_iNumber];
						t.m_lstUnvisitedCities.Remove(t.m_lstUnvisitedCities.ElementAt(iInsert));
					}
					RemovedCities.Add(tmpCity);
				}
			}
			t.m_lstUnvisitedCities.AddRange(RemovedCities);
			return t;
		}

		private Path GreedyRandomLocalSearch2(int numberOfRouts)
        {
            int i = 0;
            List<Path> lstRouts = new List<Path>();
            for (i = 0; i < numberOfRouts; i++)
            {
                lstRouts.Add(MetodaGreedyRand());
            }
           // lstRouts = lstRouts.OrderByDescending(x => x.m_dSumProfit).ToList();
            Path best = lstRouts.ElementAt(0);
            Path newPath = null;
            i = 0;
            foreach (Path p in lstRouts)
            {
                if (i < 50)
                {
					newPath = LocalSearch(p);
                }          
                else
                {
                    break;
                }

                if (newPath.m_dSumProfit > best.m_dSumProfit)
                {
                    best = new Path(newPath);
                }
                i++;
            }
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
			startCity = m_lstCity.Find(m => m.m_iNumber == m_iStartCity);
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
            return new Path(path, distance, profit, lstUnvisdCit);
        }

        ///////////////////////////////////////////////////////////////////////////////
        // Zachłanna zwraca
        private Path MetodaGreedyRand()
        {
            double distance = 0;
            double profit = 0;
            int iDistMax = m_iMaxDistance;
            List<City> path = new List<City>();						// route
            List<City> lstUnvisdCit = new List<City>(m_lstCity);	// nieodwiedzone miasta
            City currentCity = m_lstCity.Find(m => m.m_iNumber == m_iStartCity);
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
        // Wyszukujemy najlepsze miasto dzieląc profit przez dystans (z powrotem)
        private City GetBestCityWithRet(City current, City previous, List<City> unvistedNodes, Double distance, out double bestProfit, out double newDistance)
        {
			City best = null;
            bestProfit = 0;
			newDistance = 0;
            foreach (var n in unvistedNodes)
            {
                if (current != n)
                {
                    if (m_arrDistances[current.m_iNumber, n.m_iNumber] != double.MaxValue)
                    {
						if(m_arrDistances[previous.m_iNumber, n.m_iNumber] + distance + m_arrDistances[current.m_iNumber, n.m_iNumber] - m_arrDistances[current.m_iNumber, previous.m_iNumber] <= m_iMaxDistance)
						{
							if ((n.m_dProfit) / (m_arrDistances[current.m_iNumber, n.m_iNumber] + m_arrDistances[previous.m_iNumber, n.m_iNumber] - m_arrDistances[current.m_iNumber, previous.m_iNumber]) > bestProfit)
							{
								newDistance = (m_arrDistances[current.m_iNumber, n.m_iNumber] + m_arrDistances[previous.m_iNumber, n.m_iNumber]) + distance - m_arrDistances[current.m_iNumber, previous.m_iNumber];
								best = n;
								bestProfit = (n.m_dProfit) / (m_arrDistances[current.m_iNumber, n.m_iNumber] + m_arrDistances[previous.m_iNumber, n.m_iNumber] - m_arrDistances[current.m_iNumber, previous.m_iNumber]);
							}
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
            City best = null;
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
                            (m_arrDistances[current.m_iNumber, n.m_iNumber] + m_arrDistances[m_iStartCity, n.m_iNumber] + dCurrentDistance <= m_iMaxDistance))
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
            List<int> lstNumbers = new List<int>();
            foreach (var n in unvisitedCities)
            {
                if (current != n)
                {
                    if (n.m_dProfit == 0)
                        continue;
                    if (bestDist > m_arrDistances[current.m_iNumber, n.m_iNumber])
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
                bestCity = lstRandom.ElementAt(m_random.Next(0, iSize));
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
            //path.m_lstUnvisitedCities = path.m_lstUnvisitedCities.OrderByDescending(x => x.m_dProfit).ToList();
            Path bestPath = path;
            Path tmp = null;
            bool bImprove = true;
			tmp = TwoOpt(bestPath);
            while (bImprove)
            {
                bImprove = false; 
                if (tmp.m_dSumDistance <= bestPath.m_dSumDistance)
                {
                    tmp = Insert2(tmp, m_iMaxDistance);
                    if (tmp.m_dSumProfit > bestPath.m_dSumProfit)
                    {
                        bImprove = true;
                        bestPath = new Path(tmp);
                    }
                }
            }
            return new Path(bestPath);
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
            dDistance -= m_arrDistances[route.ElementAt(n - 1).m_iNumber, route.ElementAt(n).m_iNumber];
            dDistance += m_arrDistances[route.ElementAt(n - 1).m_iNumber, v];
            dDistance += m_arrDistances[v, route.ElementAt(n).m_iNumber];
            return dDistance;
        }
        private List<City> optSwap(List<City> route, int i, int k)
        {
            List<City> newPath = new List<City>();
            List<City> temp = new List<City>();
            List<City> order = new List<City>();


            //1.take route[0] to route[i - 1] and add them in order to new_route
            for (int a = 0; a < i; a++)
            {
                newPath.Add(route.ElementAt(a));
            }
            //2.take route[i] to route[k] and add them in reverse order to new_route
            for (int b = i; b < k + 1; b++)
            {
                temp.Add(route.ElementAt(b));
            }
            temp.Reverse();
            newPath.AddRange(temp);
            //3.take route[k + 1] to end and add them in order to new_route
            for (int c = k + 1; c < route.Count; c++)
            {
                newPath.Add(route.ElementAt(c));
            }

            return newPath;
        }

        private double CalcDistance(List<City> lstCities)
        {
            double dist = 0;
            for (int i = 0; i < lstCities.Count - 1; i++)
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
            List<City> bestPath = new List<City>(path.m_lstVisitedCities);
            double bestProfit = path.m_dSumProfit;
            double bestDistance = path.m_dSumDistance;
            double newDistance = bestDistance;

            bool improve = true;
            while (improve)
            {
                improve = false;
                for (int v = 0; v < unvisited.Count - 1; v++)
                {
                    //notMatch = unvisited;
                    if (unvisited.ElementAt(v).m_dProfit == 0)
                        continue;
                    for (int i = 1; i < n - 1; i++)
                    {
                        if (v > unvisited.Count - 1)
                        {
                            improve = false;
                            v = unvisited.Count;
                            break;
                        }
                        //City bestCity = GetBestCity(bestPath.ElementAt(i), notMatch);
                        newDistance = optCheckInsertNeeded(bestPath, bestDistance, unvisited.ElementAt(v).m_iNumber, i);
                        if (newDistance <= max)
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

		 private Path Insert2(Path path, double max)
        {
            int n = path.m_lstVisitedCities.Count;
            List<City> tempPath = new List<City>();
            List<City> unvisited = new List<City>(path.m_lstUnvisitedCities);
            List<City> bestPath = new List<City>(path.m_lstVisitedCities);
            double bestProfit = path.m_dSumProfit;
            double bestDistance = path.m_dSumDistance;
            double newDistance = bestDistance;
			Dictionary<int, City> mapBestCities = new Dictionary<int, City>();
            bool improve = true;
            while (improve)
            {
				double NewDistanceCheck = 0;
                improve = false;
				double bestPriftPerDistance = 0;
				double actualBestProfit = 0;
				double actualNewDistanceCheck = 0;
				City actualBestCity = null;
				int CityNumber = 0;
				for (int i = 1; i < n - 1; i++)
				{
					City bestCity = GetBestCityWithRet(bestPath.ElementAt(i), bestPath.ElementAt(i-1), unvisited, bestDistance, out bestPriftPerDistance, out NewDistanceCheck);
					if( bestCity != null && bestPriftPerDistance > actualBestProfit )
					{
						actualBestProfit = bestPriftPerDistance;
						actualBestCity = new City(bestCity);
						actualNewDistanceCheck = NewDistanceCheck;
						CityNumber = i;
					}
				}
				if (actualBestCity != null)
				{
					newDistance = optCheckInsertNeeded(bestPath, bestDistance, actualBestCity.m_iNumber, CityNumber);
					if (newDistance <= max)
					{
						tempPath = ConstructNewPath(CityNumber, actualBestCity, bestPath);
						bestProfit = bestProfit + actualBestCity.m_dProfit;
						bestDistance = newDistance;
						bestPath = tempPath;
						improve = true;
						unvisited.Remove(unvisited.Find(x=>x.m_iNumber == actualBestCity.m_iNumber));
						return new Path(bestPath, bestDistance, bestProfit, unvisited);
					}
				}
			}
			return path;
        }

        private List<City> ConstructNewPath(int insertIdx, City insertNode, List<City> currentPath)
        {
            List<City> newPath = new List<City>();

            //add first half of current path
            for (int i = 0; i < insertIdx; i++)
            {
                newPath.Add(currentPath.ElementAt(i));
            }
            //add node to list on insert index
            newPath.Add(insertNode);
            for (int i = insertIdx; i < currentPath.Count; i++)
            {
                newPath.Add(currentPath.ElementAt(i));
            }
            return newPath;
        }
    }
}