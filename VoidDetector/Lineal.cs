using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VoidDetector
{
    public class Lineal
    {
        public List<Sector> sectors { get; set; }
        public string imagePath { get; set; }

        public Lineal()
        {
            //imagePath = Path.Combine(Environment.CurrentDirectory, @"assets\imatgesToProcess");
            sectors = new List<Sector>();
        }

    }

    public class Sector
    {
        public int x { get; set; }
        public int y { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string nomSector { get; set; }

        public Sector(string nomSec)
        {
            nomSector = nomSec + "_8.jpg";

            width = 60;
            height = 60;

            switch (nomSec)
            {
                case "A1":
                    x = 5;
                    y = 900;                      
                    break;
                case "A2":
                    x = 50;
                    y = 836;  
                    break;
                case "A3":
                    x = 100;
                    y = 772;     
                    break;
                case "A4":
                    x = 157;
                    y = 705;     
                    break;
                case "A5":
                    x = 231;
                    y = 622;
                    break;
                case "A6":
                    x = 314;
                    y = 550;     
                    break;
                case "A7":
                    x = 389;
                    y = 483;    
                    break;
                case "A8":
                    x = 433;
                    y = 441;          
                    break;
                case "A9":
                    x = 486;
                    y = 395;          
                    break;
                case "A10":
                    x = 557;
                    y = 341;      
                    break;
                case "A11":
                    x = 608;
                    y = 300;        
                    break;
                case "A12":
                    x = 670;
                    y = 255;           
                    break;
                case "A13":
                    x = 736;
                    y = 199;            
                    break;
                case "A14":
                    x = 812;
                    y = 151;               
                    break;
                case "A15":
                    x = 873;
                    y = 113;               
                    break;
                case "B1":
                    x = 1;
                    y = 1020;               
                    break;
                case "B2":
                    x = 25;
                    y = 994;                
                    break;
                case "B3":
                    x = 50;
                    y = 963;                   
                    break;
                case "B4":
                    x = 69;
                    y = 927;                   
                    break;
                case "B5":
                    x = 106;
                    y = 887;                   
                    break;
                case "B6":
                    x = 146;
                    y = 843;                   
                    break;
                case "B7":
                    x = 202;
                    y = 786;                   
                    break;
                case "B8":
                    x = 246;
                    y = 728;                    
                    break;
                case "B9":
                    x = 297;
                    y = 683;                    
                    break;
                case "B10":
                    x = 342;
                    y = 639;                   
                    break;
                case "B11":
                    x = 388;
                    y = 585;                   
                    break;
                case "B12":
                    x = 450;
                    y = 531;                    
                    break;
                case "B13":
                    x = 515;
                    y = 472;                   
                    break;
                case "B14":
                    x = 563;
                    y = 426;                    
                    break;
                case "B15":
                    x = 613;
                    y = 384;                   
                    break;
                case "B16":
                    x = 653;
                    y = 358;                   
                    break;
                case "B17":
                    x = 698;
                    y = 317;                    
                    break;
                case "B18":
                    x = 768;
                    y = 278;                   
                    break;
                case "B19":
                    x = 813;
                    y = 237;                 
                    break;
                case "B20":
                    x = 861;
                    y = 197;
                    break;
                case "C1":
                    x = 122;
                    y = 1000;                    
                    break;
                case "C2":
                    x = 182;
                    y = 941;                    
                    break;
                case "C3":
                    x = 230;
                    y = 879;                    
                    break;
                case "C4":
                    x = 278;
                    y = 830;                   
                    break;
                case "C5":
                    x = 331;
                    y = 788;                   
                    break;
                case "C6":
                    x = 400;
                    y = 700;                    
                    break;
                case "C7":
                    x = 436;
                    y = 657;                   
                    break;
                case "C8":
                    x = 473;
                    y = 625;                   
                    break;
                case "C9":
                    x = 508;
                    y = 589;                   
                    break;
                case "C10":
                    x = 548;
                    y = 555;                    
                    break;
                case "C11":
                    x = 614;
                    y = 497;                    
                    break;
                case "C12":
                    x = 662;
                    y = 450;                    
                    break;
                case "C13":
                    x = 715;
                    y = 400;                   
                    break;
                case "C14":
                    x = 775;
                    y = 352;                   
                    break;
                case "C15":
                    x = 821;
                    y = 314;                    
                    break;
                case "C16":
                    x = 864;
                    y = 278;                    
                    break;
                case "D1":
                    x = 195;
                    y = 1021;                    
                    break;
                case "D2":
                    x = 225;
                    y = 995;                   
                    break;
                case "D3":
                    x = 261;
                    y = 955;                   
                    break;
                case "D4":
                    x = 302;
                    y = 921;                   
                    break;
                case "D5":
                    x = 338;
                    y = 868;                   
                    break;
                case "D6":
                    x = 389;
                    y = 823;                    
                    break;
                case "D7":
                    x = 444;
                    y = 754;                   
                    break;
                case "D8":
                    x = 516;
                    y = 687;                    
                    break;
                case "D9":
                    x = 562;
                    y = 635;                    
                    break;
                case "D10":
                    x = 605;
                    y = 594;                    
                    break;
                case "D11":
                    x = 653;
                    y = 543;                    
                    break;
                case "D12":
                    x = 518;
                    y = 486;                    
                    break;
                case "D13":
                    x = 778;
                    y = 427;                   
                    break;
                case "D14":
                    x = 829;
                    y = 383;                    
                    break;
                case "D15":
                    x = 871;
                    y = 346;                   
                    break;
                case "E1":
                    x = 317;
                    y = 1019;                    
                    break;
                case "E2":
                    x = 378;
                    y = 982;                    
                    break;
                case "E3":
                    x = 432;
                    y = 912;                    
                    break;
                case "E4":
                    x = 484;
                    y = 883;                    
                    break;
                case "E5":
                    x = 511;
                    y = 837;                    
                    break;
                case "E6":
                    x = 553;
                    y = 802;                   
                    break;
                case "E7":
                    x = 586;
                    y = 762;                   
                    break;
                case "E8":
                    x = 628;
                    y = 711;                   
                    break;
                case "E9":
                    x = 667;
                    y = 672;                   
                    break;
                case "E10":
                    x = 708;
                    y = 627;                   
                    break;
                case "E11":
                    x = 752;
                    y = 580;                   
                    break;
                case "E12":
                    x = 802;
                    y = 488;                   
                    break;
                case "E13":
                    x = 894;
                    y = 419;                   
                    break;

                default:
                    break;
            }
               
        }

    }

    //public enum NomSector
    //{
    //    A1,
    //    A2,
    //    A3,
    //    A4,
    //    A5,
    //    A6,
    //    A7,
    //    A8,
    //    A9,
    //    A10,
    //    A11,
    //    A12,
    //    A13,
    //    A14,
    //    A15,
    //    B1,
    //    B2,
    //    B3,
    //    B4,
    //    B5,
    //    B6,
    //    B7,
    //    B8,
    //    B9,
    //    B10,
    //    B11,
    //    B12,
    //    B13,
    //    B14,
    //    B15,
    //    B16,
    //    B17,
    //    B18,
    //    B19,
    //    B20,
    //    C1,
    //    C2,
    //    C3,
    //    C4,
    //    C5,
    //    C6,
    //    C7,
    //    C8,
    //    C9,
    //    C10,
    //    C11,
    //    C12,
    //    C13,
    //    C14,
    //    C15,
    //    C16,
    //    D1,
    //    D2,
    //    D3,
    //    D4,
    //    D5,
    //    D6,
    //    D7,
    //    D8,
    //    D9,
    //    D10,
    //    D11,
    //    D12,
    //    D13,
    //    D14,
    //    D15,
    //    E1,
    //    E2,
    //    E3,
    //    E4,
    //    E5,
    //    E6,
    //    E7,
    //    E8,
    //    E9,
    //    E10,
    //    E11,
    //    E12,
    //    E13,    
    //}

}
