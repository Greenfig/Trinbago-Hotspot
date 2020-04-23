using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Trinbago_MVC5.Controllers;

namespace Trinbago_MVC5.Models
{
    /// <summary>
    /// Custom validation for AdInfo collection
    /// </summary>
    sealed public class AdInfoValidateAttribute : ValidationAttribute, IClientValidatable
    {
        public ICollection<string> ErrorMessage { get; set; }
        public ICollection<string> ErrorMessageResourceName { get; set; }
        public Regex regex_Price { get; set; }
        public Regex regex_Phone { get; set; }
        public Regex regex_Mile { get; set; }
        public Regex regex_Year { get; set; }
        public Regex regex_modEngineSize { get; set; }
        public Regex regex_Size { get; set; }
        public Regex regex_Age { get; set; }
        public int MaxLength { get; set; }

        public AdInfoValidateAttribute()
        {
            // Internal RegEx
            //------------------------------------------------------------------------------------
            MaxLength = 40;
            // Cars
            regex_Mile = new Regex(@"^([1-9]{1}[0-9]{0,2}(\,[0-9]{3})*|([1-9]{1}[0-9]*))$");
            regex_Year = new Regex(@"^([1-2]{1}[0-9]{3})$");
            regex_modEngineSize = new Regex(@"^([0-9]{1}\.[0-9]{1}\s?[L])$|^([1-9]{1}[0-9]{1}\s?[L])$|^([1-9]{1}[0-9]{1,4}\s?[c][c])$|^(([V]|[v])([6]|[8]|[1][2]))$");
            regex_Size = new Regex(@"^([1-9]{1}[0-9]{0,2}(\,[0-9]{3})*|([1-9]{1}[0-9]*))$");
            // Pets
            regex_Age = new Regex(@"^[1-9]?[0-9]?[0-9]$");
        }

        public override bool IsValid(object value)
        {
            ErrorMessage = new List<string>();
            ErrorMessageResourceName = new List<string>();
            var AdInfos = value as ICollection<InfoForm>;
            bool isValid = true;
            foreach (var AdInfo in AdInfos)
            {
                // ----------------------CARS------------------------------
                if (AdInfo.Name.Equals("Make"))
                {
                    if (String.IsNullOrEmpty(AdInfo.Description))
                        isValid = false;
                }
                else if (AdInfo.Name.Equals("Model"))
                {
                    if (String.IsNullOrEmpty(AdInfo.Description))
                        isValid = false;
                }
                else if (AdInfo.Name.Equals("Year"))
                {
                    if (String.IsNullOrEmpty(AdInfo.Description))
                        isValid = false;
                    else
                    {
                        if (!regex_Year.Match(AdInfo.Description).Success)
                            isValid = false;
                    }
                }
                else if (AdInfo.Name.Equals("Mileage"))
                {
                    if (String.IsNullOrEmpty(AdInfo.Description))
                    {
                        
                        ErrorMessage.Add("Mileage is required.");
                        ErrorMessageResourceName.Add("Mileage");
                        isValid = false;
                    }
                    else
                    {
                        if (!regex_Mile.Match(AdInfo.Description).Success)
                            isValid = false;
                    }
                }
                else if (AdInfo.Name.Equals("Engine Size"))
                {
                    if (!String.IsNullOrEmpty(AdInfo.Description))
                    {
                        if (!regex_modEngineSize.Match(AdInfo.Description).Success) 
                            isValid = false;
                    }
                }
                else if (AdInfo.Name.Equals("Size"))
                {
                    if (!String.IsNullOrEmpty(AdInfo.Description))
                    {
                        if (!regex_Size.Match(AdInfo.Description).Success)                        
                            isValid = false;

                    }
                }

                // ---------------------------------PETS------------------------------
                else if (AdInfo.Name.Equals("Age"))
                {
                    if (!String.IsNullOrEmpty(AdInfo.Description))
                    {
                        if (!regex_Age.Match(AdInfo.Description).Success)                        
                            isValid = false;
                    }
                }                
            }
            return isValid;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            throw new NotImplementedException();
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
            filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            filterContext.HttpContext.Response.Cache.SetNoStore();

            base.OnResultExecuting(filterContext);
        }
    }

    public class CompressAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var encodingsAccepted = filterContext.HttpContext.Request.Headers["Accept-Encoding"];
            if (string.IsNullOrEmpty(encodingsAccepted)) return;

            encodingsAccepted = encodingsAccepted.ToLowerInvariant();
            var response = filterContext.HttpContext.Response;

            if (encodingsAccepted.Contains("deflate"))
            {
                response.AppendHeader("Content-encoding", "deflate");
                response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
            }
            else if (encodingsAccepted.Contains("gzip"))
            {
                response.AppendHeader("Content-encoding", "gzip");
                response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
            }
        }
    }

    // Lucene
    public static class LuceneSearch
    {
        private static string _luceneDir = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "lucene_index");
        private static FSDirectory _directoryTemp;
        private static FSDirectory _directory
        {
            get
            {
                if (_directoryTemp == null) _directoryTemp = FSDirectory.Open(new DirectoryInfo(_luceneDir));
                if (IndexWriter.IsLocked(_directoryTemp)) IndexWriter.Unlock(_directoryTemp);
                var lockFilePath = Path.Combine(_luceneDir, "write.lock");
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return _directoryTemp;
            }
        }
        internal static FSDirectory _getDir { get { return _directory; } }
        // Add to index
        internal static void _addToLuceneIndex(ClassifiedAd data, IndexWriter writer)
        {
            // remove older index entry
            var searchQuery = new TermQuery(new Term("Id", data.Id.ToString()));
            writer.DeleteDocuments(searchQuery);

            // add new index entry
            var doc = new Document();

            // add lucene fields mapped to db fields
            doc.Add(new Field("Id", data.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("StringId", data.StringId, Field.Store.YES, Field.Index.NOT_ANALYZED));
            // AdInfo
            foreach(var ai in data.AdInfo){
                if(!String.IsNullOrEmpty(ai.Description)){
                    if (ai.Name.Equals("Mileage"))
                    {
                        var mil = new NumericField("Mileage", Field.Store.YES, true);
                        mil.SetIntValue(ai.IntDescription);
                        doc.Add(mil);
                    }
                    else if (ai.Name.Equals("Year"))
                    {
                        var yr = new NumericField("Year", Field.Store.YES, true);
                        yr.SetIntValue(ai.IntDescription);
                        doc.Add(yr);
                    }
                    else if (ai.Name.Equals("Size"))
                    {
                        var size = new NumericField("Size", Field.Store.YES, true);
                        size.SetIntValue(ai.IntDescription);
                        doc.Add(size);
                    }
                    else
                        doc.Add(new Field(ai.Name, ai.Description, Field.Store.YES, Field.Index.NOT_ANALYZED));
                }
            }
            // Photo
            if (data.AdPhotos != null)
            {
                if (data.AdPhotos.Count > 0)
                {
                    doc.Add(new Field("AdPhoto", data.AdPhotos.SingleOrDefault(x => x.SetThumbnail == true).FileName, Field.Store.YES, Field.Index.NOT_ANALYZED));
                }
            }
            // Default
            // Title
            doc.Add(new Field("Title", data.Title, Field.Store.YES, Field.Index.ANALYZED));
            // Descripton
            var hfd = HttpUtility.HtmlDecode(data.HtmlFreeDescription);
            doc.Add(new Field("HtmlFreeDescription", hfd, Field.Store.YES, Field.Index.ANALYZED));
            
            // Price
            var price = new NumericField("Price", Field.Store.YES, true);
            price.SetIntValue(data.Price);
            doc.Add(price);
            // Price Info
            doc.Add(new Field("PriceInfo", data.PriceInfo, Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Status
            doc.Add(new Field("Status", data.Status.ToString(), Field.Store.YES, Field.Index.ANALYZED));
            // Ad Type
            doc.Add(new Field("AdType", data.AdType, Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Featured Status
            doc.Add(new Field("FeaturedAdStatus", data.FeaturedAdStatus.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Top Ad Status
            doc.Add(new Field("TopAdStatus", data.TopAdStatus.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Urgent Ad Status
            doc.Add(new Field("UrgentAdStatus", data.UrgentAdStatus.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

            // Category
            doc.Add(new Field("CategoryId", data.Category.Id.ToString(), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("CategoryName", data.Category.Name, Field.Store.YES, Field.Index.ANALYZED));
            // SubCategory
            doc.Add(new Field("SubCategoryStringId", data.SubCategory.stringId, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("SubCategoryName", data.SubCategory.Name, Field.Store.YES, Field.Index.ANALYZED));
            // Time
            doc.Add(new Field("TimeStamp", data.TimeStamp.ToShortDateString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Time Tick
            var tick = new NumericField("EditTimeStampTicks", Field.Store.YES, true);
            tick.SetLongValue(data.EditTimeStamp.Ticks);
            doc.Add(tick);
            doc.Add(new Field("EditTimeStamp", data.EditTimeStamp.ToShortDateString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Location
            doc.Add(new Field("CountryId", data.Country.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("CountryName", data.Country.Name, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("RegionId", data.Region.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("RegionName", data.Region.Name, Field.Store.YES, Field.Index.ANALYZED));
            
            // add entry to index
            writer.AddDocument(doc);
        }

        // Bulk Add
        internal static void AddUpdateLuceneIndex(IEnumerable<ClassifiedAd> data)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add data to lucene search index (replaces older entry if any)
                foreach (var sampleData in data) _addToLuceneIndex(sampleData, writer);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }

        internal static void AddUpdateLuceneIndex(ClassifiedAd sampleData)
        {
            AddUpdateLuceneIndex(new List<ClassifiedAd> { sampleData });
        }

        internal static void Optimize()
        {
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        } 

        // Map to ClassifiedAdList
        internal static ClassifiedAdList _mapLuceneDocumentToData(Document doc)
        {
            ClassifiedAdList toret = new ClassifiedAdList();
            toret.StringId = doc.Get("StringId");
            toret.Title = doc.Get("Title");
            var hfd = doc.Get("HtmlFreeDescription");
            hfd = (hfd != null ? hfd.Length > 100 ? hfd.Substring(0, 100) + "..." : hfd.TrimEnd(',', ' ') : hfd);
            toret.HtmlFreeDescription = hfd;
            var temp = new List<InfoForm>(){
                new InfoForm() { Name = "Make" , Description = doc.Get("Make")},
                new InfoForm() { Name = "Model" , Description = doc.Get("Model")},
                new InfoForm() { Name = "Bedrooms" , Description = doc.Get("Bedrooms")},
                new InfoForm() { Name = "Bathrooms" , Description = doc.Get("Bathrooms")},
                new InfoForm() { Name = "Company Name" , Description = doc.Get("Company Name")},
                new InfoForm() { Name = "Job Type" , Description = doc.Get("Job Type")},
                new InfoForm() { Name = "Species" , Description = doc.Get("Species")},
                new InfoForm() { Name = "Breed" , Description = doc.Get("Breed")}
            };
            foreach(var t in temp)
                if(!String.IsNullOrEmpty(t.Description))
                    toret.AdInfo.Add(t);
            toret.AdPhoto = new PhotoBase();
            if (!String.IsNullOrEmpty(doc.Get("AdPhoto")))
                toret.AdPhoto.FileName = doc.Get("AdPhoto");
            else
                toret.AdPhoto = null;
            toret.CategoryId = Int32.Parse(doc.Get("CategoryId"));
            toret.CategoryName = doc.Get("CategoryName");
            toret.SubCategoryStringId = doc.Get("SubCategoryStringId");
            toret.SubCategoryName = doc.Get("SubCategoryName");
            toret.Price = doc.Get("Price");
            toret.PriceInfo = doc.Get("PriceInfo");
            var time = doc.Get("TimeStamp").Split('/');
            toret.TimeStamp = new DateTime(Int32.Parse(time[2]), Int32.Parse(time[0]), Int32.Parse(time[1]));

            return toret;
        }

        public static void ClearLuceneIndexRecord(string record_id)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // remove older index entry
                var searchQuery = new TermQuery(new Term("StringId", record_id));
                writer.DeleteDocuments(searchQuery);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }

        public static void ClearAllLuceneIndexRecords()
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteAll();
                writer.Commit();
                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }

        internal static IEnumerable<ClassifiedAdList> _mapLuceneToDataList(IEnumerable<Document> hits)
        {
            return hits.Select(_mapLuceneDocumentToData).ToList();
        }
        internal static IEnumerable<ClassifiedAdList> _mapLuceneToDataList(IEnumerable<ScoreDoc> hits,
            IndexSearcher searcher)
        {
            return hits.Select(hit => _mapLuceneDocumentToData(searcher.Doc(hit.Doc))).ToList();
        }
        internal static Query parseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }
        internal static FuzzyQuery fparseQuery(string searchQuery)
        {

            var query = new FuzzyQuery(new Term("Content", searchQuery), 0.5f);
            return query;
        }
    }
}
