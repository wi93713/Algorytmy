using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorytmy2
{
    class City
    {
        public int		m_iNr			{ get; set; }	// numer punktu 0,1,2.....
		public string	m_sName			{ get; set; }	// nazwa miasta (puste typu danych Punkt) 
        public double	X				{ get; set; }	// Wspolrzedne geograficzne Latidute lub punkt X
        public double	Y				{ get; set; }	// Wspolrzedne geograficzne Longitude lub punkt Y 
		public double	m_dProfit		{ get; set; }	// Zysk 
		public bool		m_bWasVisited	{ get; set; }	// Czy był odwiedzony
    }
	class Path
    {
        public List<City> m_lstCity { get; set; }
        public double m_dSumProfit { get; set; }
        public int m_iSumRoad { get; set; }
        
    }
}
