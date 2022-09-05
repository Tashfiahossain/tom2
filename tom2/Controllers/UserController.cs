using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tom2.Models;

namespace tom2.Controllers
{
    public class UserController : Controller
    {
        tom2Entities db = new tom2Entities();
        // GET: User
        public ActionResult Index(int? page)
        {
            if (TempData["cart"] != null)
            {
                double x = 0;
                List<cart> li2 = TempData["cart"] as List<cart>;
                foreach (var item in li2)
                {
                    x += Convert.ToInt32(item.o_bill);
                }
                TempData["total"] = x;
            }
            TempData.Keep();
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.categories.Where(x => x.cat_status == 1).OrderBy(x => x.cat_id).ToList();
            IPagedList<category> cate = list.ToPagedList(pageindex, pagesize);
            return View(cate);

        }


        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(usertable us, HttpPostedFileBase imgfile)
        {
            string path = uploadimage(imgfile);

            if (path.Equals("-1"))
            {
                ViewBag.error = "Image Could Not Be Uploaded";

            }
            else
            {
                usertable u = new usertable();
                u.u_name = us.u_name;
                u.u_password = us.u_password;
                u.u_email = us.u_email;
                u.u_contact = us.u_contact;
                u.u_image = path;
                db.usertables.Add(u);
                db.SaveChanges();

                return RedirectToAction("Login");
            }
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(usertable ut)
        {
            usertable ad = db.usertables.Where(x => x.u_email == ut.u_email && x.u_password == ut.u_password).SingleOrDefault();
            if (ad != null)
            {
                Session["u_id"] = ad.u_id.ToString();
                Session["User"] = ad.u_name;
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.error = "Invalid Username or Password";
            }
            return View();
        }


        public ActionResult DisplayAddU(int? id, int? page)
        {
            TempData.Keep();
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.products.Where(x => x.cat_id_fk == id).OrderBy(x => x.pro_id).ToList();
            IPagedList<product> cate = list.ToPagedList(pageindex, pagesize);

            return View(cate);
        }


        [HttpPost]
        public ActionResult DisplayAddU(int? id, int? page, string search)
        {
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.products.Where(x => x.pro_name.Contains(search)).OrderByDescending(x => x.pro_id).ToList();
            IPagedList<product> cate = list.ToPagedList(pageindex, pagesize);

            return View(cate);
        }

        public ActionResult ViewDetail(int? id)
        {

            detail_model adm = new detail_model();

            product p = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            adm.pro_id = p.pro_id;
            adm.pro_name = p.pro_name;
            adm.pro_image = p.pro_image;
            adm.pro_price = p.pro_price;
            adm.pro_desc = p.pro_desc;

            category cat = db.categories.Where(x => x.cat_id == p.cat_id_fk).SingleOrDefault();
            adm.cat_name = cat.cat_name;
            usertable u = db.usertables.Where(x => x.u_id == p.pro_user_id_fk).SingleOrDefault();



            return View(adm);



        }


        public string uploadimage(HttpPostedFileBase file)

        {

            Random r = new Random();

            string path = "-1";

            int random = r.Next();

            if (file != null && file.ContentLength > 0)

            {

                string extension = Path.GetExtension(file.FileName);

                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))

                {

                    try

                    {



                        path = Path.Combine(Server.MapPath("~/Content/upload"), random + Path.GetFileName(file.FileName));

                        file.SaveAs(path);

                        path = "~/Content/upload/" + random + Path.GetFileName(file.FileName);



                        //    ViewBag.Message = "File uploaded successfully";

                    }

                    catch (Exception ex)

                    {

                        path = "-1";

                    }

                }

                else

                {

                    Response.Write("<script>alert('Only jpg ,jpeg or png formats are acceptable....'); </script>");

                }

            }



            else

            {

                Response.Write("<script>alert('Please select a file'); </script>");

                path = "-1";

            }







            return path;


        }


        public ActionResult Ad_tocart(int? id)
        {
           product s = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            return View(s);

        }
        List<cart> li = new List<cart>();
        [HttpPost]
        public ActionResult Ad_tocart(product sc, string qty, int id)
        {
            product s = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            cart c = new cart();
            c.pro_id = s.pro_id;
            c.pro_name = s.pro_name;
            c.pro_price = s.pro_price;
            c.o_qty = Convert.ToInt32(qty);
            c.o_bill = c.pro_price * c.o_qty;

            if (TempData["cart"] == null)
            {
                li.Add(c);
                TempData["cart"] = li;
            }
            else
            {
                List<cart> li2 = TempData["cart"] as List<cart>;
                int flag = 0;
                foreach (var item in li2)
                {
                    if (item.pro_id == c.pro_id)
                    {


                        item.o_qty += c.o_qty;
                        item.o_bill += c.o_bill;
                        flag = 1;
                    }


                }

                if (flag == 0)
                {
                    li2.Add(c);

                    // item is new......
                }


                TempData["cart"] = li2;

            }

            TempData.Keep();




            return RedirectToAction("Index");

        }


        public ActionResult checkout()
        {
            TempData.Keep();

            return View();

        }


        [HttpPost]

        public ActionResult checkout(order_table O)
        {
            List<cart> li = TempData["cart"] as List<cart>;
            invoicetable iv = new invoicetable();
            iv.in_fk_pro = Convert.ToInt32(Session["u_id"].ToString());
            iv.in_date = System.DateTime.Now;
            iv.in_totalbill = (Convert.ToInt32(TempData["total"]));
            db.invoicetables.Add(iv);
            //db.SaveChanges();

            foreach (var item in li)
            {

                order_table od = new order_table();
                od.o_fk_pro = item.pro_id;
                od.o_fk_invoice = iv.in_id;
                od.o_date = System.DateTime.Now;
                od.o_qty = item.o_qty;
                od.o_unitprice = item.pro_price;
                od.o_bill = item.o_bill;
                db.order_table.Add(od);
                db.SaveChanges();

            }
            TempData.Remove("total");
            TempData.Remove("cart");

            TempData["msg"] = "Transaction Successfully Completed........";
            TempData.Keep();



            return RedirectToAction("Index");

        }


        public ActionResult SignOut()
        {


            Session.RemoveAll();
            Session.Abandon();

            return RedirectToAction("Index");
        }





    }
}