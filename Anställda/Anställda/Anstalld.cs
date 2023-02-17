using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anställda
{
    internal class Anstalld
    {
        public int Anstallningsid { get; set; }
        public string Fornamn { get; set; }
        public string Efternamn { get; set; }
        public string Address { get; set; }
        public string Postnummer { get; set; }
        public string Kon { get; set; }
        public DateTime Fodelsedatum { get; set; }
        public string Email { get; set; }
        public string Telefonnummer { get; set; }

        public static List<Anstalld> anstalld = new List<Anstalld>();
        

        public Anstalld(int anstallningsid, string fornamn, string efternamn, string address, string postnummer, string kon, DateTime fodelsedatum, string email, string telefonummer) 
        {
            Anstallningsid = anstallningsid;
            Fornamn = fornamn; 
            Efternamn = efternamn;
            Address = address;
            Postnummer = postnummer;
            Kon = kon;
            Fodelsedatum = fodelsedatum; 
            Email = email;
            Telefonnummer= telefonummer;
        }


    }
}
