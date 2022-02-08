using LINQSamples.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LINQSamples
{
    class CustomerModel
    {
        public CustomerModel()
        {
            this.Orders = new List<OrderModel>();
        }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int OrderCount { get; set; }
        public List<OrderModel> Orders { get; set; }
    }
    class OrderModel
    {
        public int OrderId { get; set; }
        public decimal Total { get; set; }
        public List<ProductModel> Products { get; set; }
    }
    public class ProductModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public Decimal? Price { get; set; }
       // public int Quantity { get; set; } quantity alanı product tablosunda yok
    }
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new CustomNorthwindContext())
            {
                // var result = db.Database.ExecuteSqlRaw("delete from products where productId=81");
                //var result = db.Database.ExecuteSqlRaw("update products set unitPrice=unitPrice*1.2 where categoryId=4");
                //var query = "4";
                //var products = db.Products.FromSqlRaw($"select * from products where categoryId={query}").ToList();

                var products = db.ProductModels.FromSqlRaw("select ProductId,ProductName ,UnitPrice  from Products").ToList();

                foreach (var item in products)
                {
                    Console.WriteLine(item.Name+"=>"+item.Price);
                }
                
            }
            Console.ReadLine();
        }

        private static void ÇokluTablo(NorthwindContext db)
        {
            //Müşterilerin verdiği sipariş toplamı?
            // var customers = db.Customers.Where(cus => cus.Orders.Any()).Select(cus => new CustomerModel()
            var customers = db.Customers.Where(cus => cus.CustomerId == "PERIC").Select(cus => new CustomerModel()
            {
                CustomerId = cus.CustomerId,
                CustomerName = cus.ContactName,
                OrderCount = cus.Orders.Count,
                Orders = cus.Orders.Select(order => new OrderModel()
                {
                    OrderId = order.OrderId,
                    Total = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice),
                    Products = order.OrderDetails.Select(od => new ProductModel()
                    {
                        ProductId = od.ProductId,
                        Name = od.Product.ProductName,
                        Price = od.UnitPrice,
                       // Quantity = od.Quantity
                    }).ToList()
                }).ToList()

            }).OrderBy(i => i.OrderCount).ToList();

            foreach (var customer in customers)
            {
                Console.WriteLine(customer.CustomerId + "-" + customer.CustomerName + "=> " + customer.OrderCount);
                Console.WriteLine("Siparişler");
                foreach (var order in customer.Orders)
                {
                    Console.WriteLine("******************************");
                    Console.WriteLine(order.OrderId + " => " + order.Total);
                    foreach (var product in order.Products)
                    {
                       // Console.WriteLine(product.ProductId + " - " + product.Name + " => " + product.Price + " => " + product.Quantity);
                    }
                }

            }
        }

        private static void Ders8(NorthwindContext db)
        {
            //var products = db.Products.Where(p => p.CategoryId == 1).ToList();

            //var products = db.Products.Where(p => p.Category.CategoryName == "Beverages").ToList();

            //foreach (var item in products)
            //{
            //    Console.WriteLine(item.ProductName + " " + item.CategoryId + " " + item.Category.CategoryName);
            //    //item.Category.CategoryName de hata verecektir.Çünkü sorguda Select ifadesinin içerisine category içerisindeki kolonlar alanlar dahil edilmedi.
            //    //Bu hatayı Include ile çözebiliriz.
            //    var products1 = db.Products.Include(p => p.Category).Where(p => p.Category.CategoryName == "Beverages").ToList();

            //}

            //include olmadan da categoryName i aşağıdaki gibi getirebiliriz.

            //var products = db.Products.Where(p => p.Category.CategoryName == "Beverages")
            //    .Select(p => new
            //    {
            //        name = p.ProductName,
            //        id = p.CategoryId,
            //        categoryName = p.Category.CategoryName
            //    }).ToList();

            //foreach (var item in products)
            //{
            //    Console.WriteLine(item.name + " " + item.id + " " + item.categoryName);
            //}

            //var categories = db.Categories.Where(c => c.Products.Count() == 0).ToList();

            //en az bir ürünü olan ürünleri getirir
            //var categories = db.Categories.Where(c => c.Products.Any()).ToList();

            ////left join
            //var products = db.Products
            //    .Select(p=> 
            //    new 
            //    {
            //        companyName=p.Supplier.CompanyName,
            //        contactName=p.Supplier.ContactName,
            //        productName=p.ProductName
            //    }).ToList();

            //foreach (var item in products)
            //{
            //    Console.WriteLine(item.productName+" "+item.companyName+" "+item.contactName);
            //}

            //extensions methods
            //query expressions

            //var products = (from p in db.Products
            //                where p.UnitPrice>10
            //               select p).ToList();
            ////db.Products.Where(p=>p.UnitPrice>10).ToList();
            ///

            //inner join
            var products = (from p in db.Products
                            join s in db.Suppliers on p.SupplierId equals s.SupplierId
                            select new
                            {
                                ProductName = p.ProductName,
                                CompanyName = s.CompanyName,
                                ContactName = s.ContactName
                            }).ToList();
        }

        private static void Ders7(NorthwindContext db)
        {
            //var product = db.Products.Find(88);

            //if (product!=null)
            //{
            //    db.Products.Remove(product);
            //    db.SaveChanges();
            //}

            //var product = new Product() { ProductId = 87 };
            //db.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;

            //db.SaveChanges();

            var product1 = new Product() { ProductId = 87 };
            var product2 = new Product() { ProductId = 88 };

            var products = new List<Product>() { product1, product2 };
            db.Products.RemoveRange(products);
            db.SaveChanges();
        }

        private static void Ders6(NorthwindContext db)
        {
            //var product = db.Products.FirstOrDefault(p=>p.ProductId==1);

            //if (product!=null)
            //{
            //    product.UnitsInStock += 10;
            //    db.SaveChanges();

            //    Console.WriteLine("veri güncellendi");
            //}

            //var product = new Product() { ProductId = 1 };
            //db.Products.Attach(product);

            //product.UnitsInStock = 50;
            //db.SaveChanges();

            var product = db.Products.Find(1);


            if (product != null)
            {
                product.UnitPrice = 28;
                db.Update(product);
                db.SaveChanges();
            }
        }

        private static void Ders5(NorthwindContext db)
        {
            var category = db.Categories.Where(i => i.CategoryName == "Beverages").FirstOrDefault();
            category.Products.Add(new Product() { ProductName = "Yeni ürünnn1" }); // ilişkilendirilerek ekleme.
            category.Products.Add(new Product() { ProductName = "Yeni ürünnn2" });


            //var product1 = new Product() { ProductName = "yeni ürün3",Category=new Category() {CategoryName="yeni kategori" } };
            //var product2 = new Product() { ProductName = "yeni ürün4",Category=category };

            //db.Products.Add(product1);
            //db.SaveChanges();

            //var products = new List<Product>() 
            //{
            //    new Product(){ProductName="Yeni ürün 1"},
            //    new Product(){ProductName="Yeni ürün 2"}
            //};

            //db.Products.AddRange(products); // add with list
            //Console.WriteLine("veri eklendi");
            //Console.WriteLine(product1.ProductId);
        }

        private static void Ders4(NorthwindContext db)
        {
            //var result = db.Products.Count();
            //var result = db.Products.Count(i => i.UnitPrice > 10 && i.UnitPrice < 30);
            //var result = db.Products.Count(i => !i.Discontinued);

            //var result = db.Products.Min(p => p.UnitPrice);
            //var result = db.Products.Max(p => p.UnitPrice);
            //var result = db.Products.Where(p=>p.CategoryId==1).Max(p => p.UnitPrice);

            //var result = db.Products.Where(p=>!p.Discontinued).Average(p => p.UnitPrice);
            //var result = db.Products.Where(p => !p.Discontinued).Sum(p => p.UnitPrice);

            //var result = db.Products.OrderBy(p=>p.UnitPrice).ToList(); artan
            //var result = db.Products.OrderByDescending(p => p.UnitPrice).ToList(); azalan

            //var result = db.Products.OrderByDescending(p => p.UnitPrice).FirstOrDefault(); //max price

            var result = db.Products.OrderByDescending(p => p.UnitPrice).LastOrDefault(); //min price
        }

        private static void Ders3(NorthwindContext db)
        {
            //var customers = db.Customers.ToList();
            var customers = db.Customers.Select(c => new { c.CustomerId, c.ContactName }).ToList();

            //var customersInGermany = db.Customers.Where(c => c.Country == "Germany").Select(c => new { c.ContactName }).ToList();
            var customersInGermany = db.Customers.Select(c => new { c.ContactName, c.Country }).Where(c => c.Country == "Germany").ToList();
            var diegoRoelCountry = db.Customers.Where(c => c.ContactName == "Diego Roel").FirstOrDefault();
            //Console.WriteLine(diegoRoelCountry.Country);
            var productsNotInStock = db.Products.Where(p => p.UnitsInStock == 0).Select(p => new { p.ProductName }).ToList();
            var employees = db.Employees.Select(e => new { e.FirstName, e.LastName }).ToList();

            //var products = db.Products.Take(5).ToList();
            //var products = db.Products.Skip(5).Take(5).ToList();
        }

        private static void Ders2(NorthwindContext db)
        {
            //var products = db.Products.Where(p => p.UnitPrice > 18).ToList();
            //var products = db.Products.Select(p => new { p.ProductName, p.UnitPrice }).Where(p => p.UnitPrice > 18).ToList();
            //var products = db.Products.Where(p => p.UnitPrice > 18 && p.UnitPrice<30).ToList();
            //var products = db.Products.Where(p => p.CategoryId>=1 && p.CategoryId<=5).ToList();
            //var products = db.Products.Where(p => p.CategoryId == 1 || p.CategoryId == 5).ToList();
            //var products = db.Products.Where(p => p.CategoryId==1).Select(p => new { p.ProductName, p.UnitPrice }).ToList();
            //var products = db.Products.Where(i => i.ProductName == "Chai").ToList();
            var products = db.Products.Where(i => i.ProductName.Contains("Chai")).ToList();
            foreach (var p in products)
            {
                Console.WriteLine(p.ProductName + ' ' + p.UnitPrice);
            }
        }

        private static void Ders1(NorthwindContext db)
        {
            //var products = db.Products.ToList();
            //var products = db.Products.Select(p => p.ProductName).ToList();
            //var products = db.Products.Select(p => new { p.ProductName,p.UnitPrice }).ToList();
            //var products = db.Products.Select(p => new ProductModel() { Name = p.ProductName, Price = p.UnitPrice }).ToList();


            //foreach (var p in products)
            //{
            //    Console.WriteLine(p.Name + ' ' + p.Price);
            //}

            //var product = db.Products.First();
            var product = db.Products.Select(p => new { p.ProductName, p.UnitPrice }).FirstOrDefault();

            Console.WriteLine(product.ProductName + ' ' + product.UnitPrice);
        }
    }
}
