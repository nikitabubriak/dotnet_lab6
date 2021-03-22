using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

/*
 * Lab 6
 * Variant 1
 * 
 * Розробити структуру даних для зберігання інформації про книги в бібліотеці. 
 * Книга характеризується: 
 *       назвою 
 *      ,прізвищем автора 
 *      ,вартістю 
 *      ,датою видання 
 *      ,видавництвом 
 *      ,списком інвентарних номерів (книга в кількох примірниках). 
 * У одного автора може бути декілька книг.
 */

// XML
// LINQ to XML

namespace lab6
{
    class Book
    {
        public int ID { get; set; }
        public int AuthorID { get; set; }
        public int PublisherID { get; set; }
        public string Title { get; set; }
        public DateTime DatePublished { get; set; }
        public float Price { get; set; }
        public List<int> InventoryID { get; set; }

        private static bool isAddingBook = true;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Book()
        {

        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorID"></param>
        /// <param name="publisherID"></param>
        /// <param name="title"></param>
        /// <param name="datePublished"></param>
        /// <param name="price"></param>
        /// <param name="inventoryID"></param>
        public Book(int id, int authorID, int publisherID, string title, DateTime datePublished, float price, List<int> inventoryID)
        {
            ID = id;
            AuthorID = authorID;
            PublisherID = publisherID;
            Title = title;
            DatePublished = datePublished;
            Price = price;
            InventoryID = new List<int>(inventoryID);
        }

        public static void Add()
        {
            Console.WriteLine("Press Y to add book. Press N to stop adding");
            while (isAddingBook)
            {
                Program.AddObject(Program.AddBook, ref isAddingBook);
            }
        }

    }

    class Author
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<int> Books { get; set; }

        private static bool isAddingAuthor = true;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Author()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="books"></param>
        public Author(int id, string name, List<int> books)
        {
            ID = id;
            Name = name;
            Books = new List<int>(books);
        }

        public static void Add()
        {
            Console.WriteLine("Press Y to add author. Press N to stop adding");
            while (isAddingAuthor)
            {
                Program.AddObject(Program.AddAuthor, ref isAddingAuthor);
            }
        }
    }

    class Program
    {
        public static List<Book> books = new List<Book>();
        public static List<Author> authors = new List<Author>();

        static void Main(string[] args)
        {
            // Console input of data

            Book.Add();
            //Author.Add();

            // Create .xml file

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create("bookshelf.xml", settings))
            {
                writer.WriteStartElement("bookshelf");
                foreach (Book book in books)
                {
                    writer.WriteStartElement("book");
                    writer.WriteElementString("ID", book.ID.ToString());
                    writer.WriteElementString("authorID", book.AuthorID.ToString());
                    writer.WriteElementString("publisherID", book.PublisherID.ToString());
                    writer.WriteElementString("title", book.Title);
                    writer.WriteElementString("datePublished", book.DatePublished.ToString());
                    writer.WriteElementString("price", book.Price.ToString());
                    foreach (int inv in book.InventoryID)
                        writer.WriteElementString("inventoryID", inv.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            // Load .xml file using XmlDocument

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("bookshelf.xml");

            // Load and output .xml file using LINQ to XML

            Console.WriteLine($"{"ID",10} {"AuthorID",10} {"PublisherID",10} {"Title",15} {"DatePublished",25} {"Price",5}\n");

            XDocument document = XDocument.Load("bookshelf.xml");
            IEnumerable<XElement> booksXML = document.Element("bookshelf").Elements("book");
            foreach (XElement book in booksXML)
            {
                XElement ID = book.Element("ID");
                XElement authorID = book.Element("authorID");
                XElement publisherID = book.Element("publisherID");
                XElement title = book.Element("title");
                XElement datePublished = book.Element("datePublished");
                XElement price = book.Element("price");
                IEnumerable<XElement> bookInventory = book.Elements("inventoryID");

                if (ID != null && authorID != null && publisherID != null && 
                    title != null && datePublished != null && price != null)
                {
                    Console.WriteLine($"{ID.Value,10} {authorID.Value,10} {publisherID.Value,10} " +
                        $"{title.Value,15} {datePublished.Value,25} {price.Value,5}\n");
                }
                
                Console.WriteLine($"{ "Book's Inventory ID:",30}");
                foreach (var id in bookInventory.ToList())
                {
                    if(id != null)
                        Console.Write($"{id.Value,10}");
                }
                Console.WriteLine("\n\n");

            }

            //LINQ to XML queries

            Console.WriteLine("\nProjection - select");
            var std1XML = from b in booksXML
                       select b.Element("title");
            foreach (var title in std1XML)
                Console.WriteLine(title.Value);

            var ext1XML = booksXML.Select(b => b.Element("price"));
            foreach (var price in ext1XML)
                Console.WriteLine($"${price.Value}");


            Console.WriteLine("\nOne to many - SelectMany");
            var oneToManyXML = booksXML.SelectMany
                (b => b.Elements("inventoryID"),
                (b, i) => new { BookTitle = b.Element("title"), InventoryID = i });
            foreach (var book in oneToManyXML)
                Console.WriteLine($"{book.BookTitle.Value} - {book.InventoryID.Value}");


            Console.WriteLine("\nCondition - where");
            var std2XML = from b in booksXML
                          where (int)b.Element("publisherID") % 2 == 0
                          select new { Title = b.Element("title"), Publisher = b.Element("publisherID") };
            foreach (var book in std2XML)
                Console.WriteLine($"{book.Title.Value} - {book.Publisher.Value}");

            var ext2XML = booksXML.Where(b => !System.DateTime.Equals(DateTime.Parse(b.Element("datePublished").Value), DateTime.Parse("2021 3 20")))
                                .Select(b => new { Title = b.Element("title"), Date = b.Element("datePublished") });
            foreach (var book in ext2XML)
                Console.WriteLine($"{book.Title.Value} - {book.Date.Value}");


            Console.WriteLine("\nGrouping - group by");
            var std3XML = from b in booksXML
                          group b by b.Element("publisherID");
            foreach (var b in std3XML)
            {
                Console.WriteLine("\nPublisherID:" + b.Key.Value);
                foreach (var p in b)
                    Console.WriteLine(p.Element("title").Value);
            }

            var ext3XML = booksXML.GroupBy(b => b.Element("authorID"));
            foreach (var b in ext3XML)
            {
                Console.WriteLine("\nAuthorID:" + b.Key.Value);
                foreach (var a in b)
                    Console.WriteLine(a.Element("title").Value);
            }


            Console.WriteLine("\nSorting - orderby");
            var std4XML = from b in booksXML
                          orderby b.Element("title").Value descending
                          select b.Element("title");
            foreach (var title in std4XML)
                Console.WriteLine(title.Value);

            var ext4XML = booksXML.OrderBy(b => b.Element("price").Value).Select(b => b.Element("price"));
            foreach (var price in ext4XML)
                Console.WriteLine(price.Value);


            Console.WriteLine("\nAggregation - Count");
            var countXML = booksXML.Count();
            Console.WriteLine(countXML);

        }

        // Add an object to its list in Program
        public static void AddObject(Action addObject, ref bool isAdding)
        {
            Console.WriteLine("Y/N?");
            ConsoleKeyInfo result = Console.ReadKey();
            
            if ((result.KeyChar == 'Y') || (result.KeyChar == 'y'))
            {
                addObject();
            }
            else if ((result.KeyChar == 'N') || (result.KeyChar == 'n'))
            {
                isAdding = false;
            }
        }

        // Console input for Book data
        public static void AddBook()
        {
            Console.WriteLine("Input Book ID: ");
            int ID = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Input Author ID: ");
            int authorID = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Input Publisher ID: ");
            int publisherID = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Input Title: ");
            string title = Console.ReadLine();

            Console.WriteLine("Input Date of publication(yyyy mm dd): ");
            DateTime datePublished = Convert.ToDateTime(Console.ReadLine());

            Console.WriteLine("Input Price: ");
            float price = Convert.ToSingle(Console.ReadLine());

            Console.WriteLine("Input Inventory IDs to list. Press Enter to stop input");
            List<int> inventoryID = new List<int>();
            string input = " ";
            while (input != "")
            {
                Console.WriteLine("Next: ");
                input = Console.ReadLine();
                if (input == "") break;
                inventoryID.Add(Convert.ToInt32(input));
            }
            books.Add(new Book(ID, authorID, publisherID, title, datePublished, price, inventoryID));
            Console.WriteLine("Book added");
        }

        // Console input for Author data
        public static void AddAuthor()
        {
            Console.WriteLine("Input Author ID: ");
            int ID = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Input Name: ");
            string name = Console.ReadLine();

            Console.WriteLine("Input author's Book IDs to list. Press Enter to stop input");
            List<int> bookID = new List<int>();
            string input = " ";
            while (input != "")
            {
                Console.WriteLine("Next: ");
                input = Console.ReadLine();
                if (input == "") break;
                bookID.Add(Convert.ToInt32(input));
            }

            authors.Add(new Author(ID, name, bookID));
            Console.WriteLine("Author added");
        }
    }


}
