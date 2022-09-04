using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tom2.Models
{
    public class detail_model
    {
        public int pro_id { get; set; }
        public string pro_name { get; set; }
        public string pro_image { get; set; }
        public Nullable<int> pro_price { get; set; }
        public string pro_desc { get; set; }



        public Nullable<int> cat_id_fk { get; set; }
        public Nullable<int> admin_id_fk { get; set; }

        public Nullable<int> pro_user_id_fk { get; set; }

        public int cat_id { get; set; }
        public string cat_name { get; set; }


        public string admin_name { get; set; }


        public string admin_image { get; set; }

        public string u_contact { get; set; }
        public string u_name { get; set; }


        public string u_image { get; set; }
    }
}
