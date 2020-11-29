using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VoidDetector;

namespace Web.Models
{
    public class ResumViewModel
    {
        [DisplayName("File: ")]
        public string FileName { get; set; }
        public string ImgPath { get; set; }
        [DisplayName("Obstructions: ")]
        public string ObstructionStr { get; set; }
        public int Obstruction { get; set; }
        [DisplayName("Empty spaces: ")]
        public int Empty { get; set; }
        public int EmptyPercent { get; set; }
        public int Full { get; set; }

        public List<Results> Results { get; set; }

        public ResumViewModel(List<Results> results)
        {
            this.Results = results.OrderBy(x=>x.prediction).ToList();
        }

    }
}
