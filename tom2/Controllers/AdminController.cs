using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tom2.Models;
using PagedList.Mvc;
using PagedList;
using System.Data.Entity.Validation;

namespace tom2.Controllers
{
    public class AdminController : Controller
    {
        tom2Entities db = new tom2Entities();
        [HttpGet]
        // GET: Admin
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(admintable admin)
        {
            admintable ad = db.admintables.Where(x => x.admin_name == admin.admin_name && x.admin_password == admin.admin_password).SingleOrDefault();
            if (ad != null)
            {
                Session["admin_id"] = ad.admin_id.ToString();
                return RedirectToAction("Category");
            }
            else
            {
                ViewBag.error = "Invalid Username or Password";
            }
            return View();
        }
        public ActionResult Category()
        {
            if (Session["admin_id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Category(category cat, HttpPostedFileBase imgfile)
        {

            admintable ad = new admintable();
            string path = uploadimage(imgfile);

            if (path.Equals("-1"))
            {
                ViewBag.error = "Image Could Not Be Uploaded";

            }
            else
            {
                category ca = new category();
                ca.cat_name = cat.cat_name;
                ca.cat_image = path;
                ca.cat_status = 1;
                ca.admin_id_fk = Convert.ToInt32(Session["admin_id"].ToString());



                db.categories.Add(ca);


                db.SaveChanges();


                return RedirectToAction("ViewCategory");
            }


            return View();

        }


        public ActionResult ViewCategory(int? page)
        {
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.categories.Where(x => x.cat_status == 1).OrderBy(x => x.cat_id).ToList();
            IPagedList<category> cate = list.ToPagedList(pageindex, pagesize);

            return View(cate);
        }
        //public ActionResult Index(int? page)
        //{
        //    int pagesize = 9, pageindex = 1;
        //    pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
        //    var list = db.categories.Where(x => x.cat_status == 1).OrderBy(x => x.cat_id).ToList();
        //    IPagedList<category> cate = list.ToPagedList(pageindex, pagesize);
        //    return View(cate);

        //}

        [HttpGet]
        public ActionResult CreateAdd()
        {
            List<category> li = db.categories.ToList();
            ViewBag.categoryList = new SelectList(li, "cat_id", "cat_name");
            return View();
        }

        [HttpPost]
        public ActionResult CreateAdd(product c, HttpPostedFileBase imgfile)
        {
            List<category> li = db.categories.ToList();
            ViewBag.categoryList = new SelectList(li, "cat_id", "cat_name");


            string path = uploadimage(imgfile);

            if (path.Equals("-1"))
            {
                ViewBag.error = "Image Could Not Be Uploaded";

            }
            else
            {
                product sc = new product();
                sc.pro_name = c.pro_name;
                sc.pro_price = c.pro_price;
                sc.pro_image = path;
                sc.cat_id_fk = c.admin_id_fk;
                sc.pro_desc = c.pro_desc;
                sc.admin_id_fk = Convert.ToInt32(Session["admin_id"].ToString());
                //db.sub_category.Add(sc);
                //db.SaveChanges();
                try
                {
                    db.products.Add(sc);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            System.Console.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
                Response.Redirect("ViewCategory");
            }
            return View();
        }

        public ActionResult DisplayAdd(int? id, int? page)
        {
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.products.Where(x => x.cat_id_fk == id).OrderBy(x => x.pro_id).ToList();
            IPagedList<product> cate = list.ToPagedList(pageindex, pagesize);

            return View(cate);
        }


        [HttpPost]
        public ActionResult DisplayAdd(int? id, int? page, string search)
        {
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.products.Where(x => x.pro_name.Contains(search)).OrderByDescending(x => x.pro_id).ToList();
            IPagedList<product> cate = list.ToPagedList(pageindex, pagesize);

            return View(cate);
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
        public ActionResult SignOut()
        {


            Session.RemoveAll();
            Session.Abandon();

            return RedirectToAction("ViewCategory");
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
            admintable u = db.admintables.Where(x => x.admin_id == p.admin_id_fk).SingleOrDefault();
            adm.admin_name = u.admin_name;
            //adm.u_image = u.u_image;
            // adm.u_contact = u.u_contact;
            adm.admin_id_fk = u.admin_id;
            return View(adm);



        }

        public ActionResult Add_Delete(int? id)
        {
           product p = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            db.products.Remove(p);
            db.SaveChanges();

            return RedirectToAction("ViewCategory");
        }


    }
}