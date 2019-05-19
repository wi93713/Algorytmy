using System;
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

        private bool m_bDataTypeCity		= false;           // FALSE = POINTS, TRUE = MIASTA
		private List<City> m_lstCity		= null;				// lista wczytanych miast/punktów
		
		
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
						m_iNr = i,
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
		///////////////////////////////////////////////////////////////////////////////
		/// Rysowanie punktów na canvasie
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

		///////////////////////////////////////////////////////////////////////////////
		// Zapis obliczonych ścieżek do pliku
		private void SaveData(object sender, RoutedEventArgs e)
        {

        }
	}
}
