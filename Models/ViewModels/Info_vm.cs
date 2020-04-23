using System.Collections.Generic;

namespace Trinbago_MVC5.Models
{   
    /**
     * Classified AdInfo Partial Data
     **/
    public class AdInfoPartial
    {
        //
        // Info
        public ICollection<InfoForm> AdInfo { get; set; }

        //
        // Select list
        public ICollection<SelectListForm> SelectListForm { get; set; }
    }

    public class MiscInfoNoId
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}