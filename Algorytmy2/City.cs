using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorytmy2
{
    class City
    {	
		public City() { }
		public City(City bestCity)
		{
			m_bWasVisited = bestCity.m_bWasVisited;
			m_iNumber = bestCity.m_iNumber;
			X = bestCity.X;
			Y = bestCity.Y;
			m_sName = bestCity.m_sName;
			m_dProfit = bestCity.m_dProfit;
		}

		public int		m_iNumber		{ get; set; }	// numer punktu 0,1,2.....
		public string	m_sName			{ get; set; }	// nazwa miasta (puste typu danych Punkt) 
        public double	X				{ get; set; }	// Wspolrzedne geograficzne Latidute lub punkt X
        public double	Y				{ get; set; }	// Wspolrzedne geograficzne Longitude lub punkt Y 
		public double	m_dProfit		{ get; set; }	// Zysk 
		public bool		m_bWasVisited	{ get; set; }	// Czy był odwiedzony
    }
    class Path
    {
		public List<City>	m_lstVisitedCities			{ get; set; }	// obliczona droga
		public List<City>	m_lstUnvisitedCities{ get; set; }	// nieodwiedzone miasta
        public double		m_dSumProfit		{ get; set; }	// profit
        public double		m_dSumDistance		{ get; set; }	// dystans

		public Path(List<City> path, double distance, double profit, List<City> lstUnvisdCit)
		{
			m_lstVisitedCities = path;
			m_lstUnvisitedCities = lstUnvisdCit;
			m_dSumProfit = profit;
			m_dSumDistance = distance;
		}

		public Path()
		{

		}
		public Path(Path t)
		{
			m_lstVisitedCities = new List<City>(t.m_lstVisitedCities);
			m_lstUnvisitedCities = new List<City>(t.m_lstUnvisitedCities);
			m_dSumProfit = t.m_dSumProfit;
			m_dSumDistance = t.m_dSumDistance;
		}
	}
}
