using AutoMapper;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Controllers;
using Trinbago_MVC5.Managers;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Extensions
{
    // Lucene
    public static class LuceneSearch
    {
        private static string _luceneDir = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "lucene_index");
        private static FSDirectory _directoryTemp;
        private static FSDirectory _write
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
        private static FSDirectory _read
        {
            get
            {
                if (_directoryTemp == null) _directoryTemp = FSDirectory.Open(new DirectoryInfo(_luceneDir));
                return _directoryTemp;
            }
        }
        public static FSDirectory _getDir { get { return _read; } }

        // Add to index
        public static void _createLuceneIndex(ClassifiedAdLucene data, IndexWriter writer, SeoManager ManagerSeo, Document doc)
        {
            doc = new Document();
            // add lucene fields mapped to db fields
            doc.Add(new Field("Id", data.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("StringId", data.StringId, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("PosterId", data.PosterId, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("PosterName", data.PosterName, Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Default
            // Title
            doc.Add(new Field("Title", data.Title, Field.Store.YES, Field.Index.ANALYZED));
            // Descripton
            var hfd = HttpUtility.HtmlDecode(data.HtmlFreeDescription);
            doc.Add(new Field("HtmlFreeDescription", hfd, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Description", data.Description, Field.Store.YES, Field.Index.ANALYZED));
            // Price
            var price = new NumericField("Price", Field.Store.YES, true);
            price.SetIntValue(data.Price);
            doc.Add(price);
            // Price Info
            doc.Add(new Field("PriceInfo", data.PriceInfo, Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Status
            var stat = new NumericField("Status", Field.Store.YES, true);
            stat.SetIntValue(data.Status);
            doc.Add(stat);
            // Ad Type
            doc.Add(new Field("AdType", data.AdType, Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Featured Status
            doc.Add(new Field("FeaturedAdStatus", data.AdPromotionFeaturedAdStatus.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Top Ad Status
            doc.Add(new Field("TopAdStatus", data.AdPromotionTopAdStatus.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Urgent Ad Status
            doc.Add(new Field("UrgentAdStatus", data.AdPromotionUrgentAdStatus.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Contact info
            doc.Add(new Field("ContactPrivacy", data.ContactPrivacy.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            if (!string.IsNullOrEmpty(data.AdContactName))
                doc.Add(new Field("AdContactName", data.AdContactName, Field.Store.YES, Field.Index.NOT_ANALYZED));
            if (!string.IsNullOrEmpty(data.AdContactPhone))
                doc.Add(new Field("AdContactPhone", data.AdContactPhone, Field.Store.YES, Field.Index.NOT_ANALYZED));
            if (!string.IsNullOrEmpty(data.AdContactPhone2))
                doc.Add(new Field("AdContactPhone2", data.AdContactPhone2, Field.Store.YES, Field.Index.NOT_ANALYZED));
            if (!string.IsNullOrEmpty(data.AdContactPhone3))
                doc.Add(new Field("AdContactPhone3", data.AdContactPhone3, Field.Store.YES, Field.Index.NOT_ANALYZED));
            if (!string.IsNullOrEmpty(data.AdContactEmail))
                doc.Add(new Field("AdContactEmail", data.AdContactEmail, Field.Store.YES, Field.Index.NOT_ANALYZED));
            // Category
            doc.Add(new Field("CategoryId", data.CategoryId.ToString(), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("CategoryName", data.CategoryName, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("CategorySeoName", data.CategorySeoName, Field.Store.YES, Field.Index.NO));
            // SubCategory
            doc.Add(new Field("SubCategoryId", data.SubCategoryId.ToString(), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("SubCategoryName", data.SubCategoryName, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("SubCategorySeoName", data.SubCategorySeoName, Field.Store.YES, Field.Index.NO));
            // Time Tick
            var tick = new NumericField("TimeStampTicks", Field.Store.YES, true);
            tick.SetLongValue(data.TimeStamp.Ticks);
            doc.Add(tick);
            tick = new NumericField("EditStampTicks", Field.Store.YES, false);
            tick.SetLongValue(data.EditTimeStamp.Ticks);
            doc.Add(tick);
            // Location
            var cid = new NumericField("CountryId", Field.Store.YES, true);
            cid.SetIntValue(data.CountryId);
            doc.Add(cid);
            var rid = new NumericField("RegionId", Field.Store.YES, true);
            rid.SetIntValue(data.RegionId);
            doc.Add(rid);
            doc.Add(new Field("CountryName", data.CountryName, Field.Store.YES, Field.Index.NO));
            doc.Add(new Field("RegionName", data.RegionName, Field.Store.YES, Field.Index.NO));
            // Seo Title
            doc.Add(new Field("SeoTitle", new SeoManager().GetSeoTitle(data.Title), Field.Store.YES, Field.Index.NO));
            // Seo Category
            doc.Add(new Field("SeoCategory", data.SubCategorySeoName, Field.Store.YES, Field.Index.NO));
            // Seo Location
            doc.Add(new Field("SeoLocation", data.RegionSeoName, Field.Store.YES, Field.Index.NO));
            // Lists
            // AdInfo Search Store
            foreach (var ai in data.AdInfo)
            {
                if (!string.IsNullOrEmpty(ai.Description))
                {
                    if (ai.Name.Equals("Mileage"))
                    {
                        var mil = new NumericField("Mileage", Field.Store.YES, true);
                        mil.SetIntValue(int.Parse(ai.Description.Replace(",", "")));
                        doc.Add(mil);
                    }
                    else if (ai.Name.Equals("Year"))
                    {
                        var yr = new NumericField("Year", Field.Store.YES, true);
                        yr.SetIntValue(int.Parse(ai.Description.Replace(",", "")));
                        doc.Add(yr);
                    }
                    else if (ai.Name.Equals("Size"))
                    {
                        var size = new NumericField("Size", Field.Store.YES, true);
                        size.SetIntValue(int.Parse(ai.Description.Replace(",", "")));
                        doc.Add(size);
                    }
                    else if(ai.Name.Equals("Body Type"))
                    {
                        doc.Add(new Field(ai.Name, ManagerSeo.GetSeoTitle(ai.Description.Replace("(2 door)", "")), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }
                    else if (ai.Name.Equals("Rental Type"))
                    {
                        doc.Add(new Field(ai.Name, ManagerSeo.GetSeoTitle(ai.Description), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }
                    else
                        doc.Add(new Field(ai.Name, ai.Description, Field.Store.YES, Field.Index.NOT_ANALYZED));
                }
            }
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            // AdInfo For Ad Details
            if (data.AdInfo != null && data.AdInfo.Count > 0)
            {                
                // Get adinfo excluding nulls
                var adlist = from list in data.AdInfo
                             where list.Description != null
                             select new { list.Name, list.Description };
                var adinfos = serializer.Serialize(adlist);
                doc.Add(new Field("AdInfo", adinfos, Field.Store.YES, Field.Index.NO));
            }
            // Photo For Ad Details
            // For first image store all
            // Store adlist_filename and raw_filename for rest
            if (data.Photos != null && data.Photos.Count > 0)
            {
                // For ad list thumbnail
                // convert thubmnail to byte array
                var dir = HostingEnvironment.MapPath("~/Photos/" + data.StringId.Substring(2, 4) + "/" + data.StringId.Substring(0, 4));
                var lucenepath = Path.Combine(dir, "lucene");
                System.IO.Directory.CreateDirectory(lucenepath);
                var adphotosJson = serializer.Serialize(data.Photos);
                // All other photos
                doc.Add(new Field("AdPhotos", adphotosJson, Field.Store.YES, Field.Index.NO));
                // Add Byte Array
                // Add photos
                Document images;
                foreach (var ap in data.Photos)
                {
                    if (ap.AdList_FileName != null)
                    {
                        images = new Document();
                        //var adlist = PhotoEditing.FileToByteArray(Path.Combine(dir, ap.AdList_FileName));
                        images.Add(new Field("AdPhotoLocatorId", string.Format("{0}-{1}", data.Id, ap.AdList_FileName), Field.Store.YES, Field.Index.NOT_ANALYZED));
                        //images.Add(new Field("AdPhoto", adlist, 0, adlist.Length, Field.Store.YES));
                        images.Add(new Field("FilePath", Path.Combine(lucenepath, ap.AdList_FileName), Field.Store.YES, Field.Index.NO));
                        images.Add(new Field("ContentType", ap.ContentType, Field.Store.YES, Field.Index.NO));
                        if (File.Exists(Path.Combine(lucenepath, ap.AdList_FileName)) == false)
                        {
                            try
                            {
                                File.Copy(Path.Combine(dir, ap.AdList_FileName), Path.Combine(lucenepath, ap.AdList_FileName), false);
                            }
                            catch (Exception ex) { }
                        }
                        writer.AddDocument(images);
                    }
                    images = new Document();
                    //var addetails = PhotoEditing.FileToByteArray(Path.Combine(dir, ap.AdDetails_FileName));
                    images.Add(new Field("AdPhotoLocatorId", string.Format("{0}-{1}", data.Id, ap.AdDetails_FileName), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    //images.Add(new Field("AdPhoto", addetails, 0, addetails.Length, Field.Store.YES));
                    images.Add(new Field("FilePath", Path.Combine(lucenepath, ap.AdDetails_FileName), Field.Store.YES, Field.Index.NO));
                    images.Add(new Field("ContentType", ap.ContentType, Field.Store.YES, Field.Index.NO));
                    if (File.Exists(Path.Combine(lucenepath, ap.AdDetails_FileName)) == false)
                    {
                        try
                        {
                            File.Copy(string.Format("{0}/{1}", dir, ap.AdDetails_FileName), Path.Combine(lucenepath, ap.AdDetails_FileName), false);
                        }
                        catch (Exception ex) { }
                    }
                    writer.AddDocument(images);
                    images = new Document();
                    //var adraw = PhotoEditing.FileToByteArray(Path.Combine(dir, ap.Raw_FileName));
                    images.Add(new Field("AdPhotoLocatorId", string.Format("{0}-{1}", data.Id, ap.Raw_FileName), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    //images.Add(new Field("AdPhoto", adraw, 0, adraw.Length, Field.Store.YES));
                    images.Add(new Field("FilePath", Path.Combine(lucenepath, ap.Raw_FileName), Field.Store.YES, Field.Index.NO));
                    images.Add(new Field("ContentType", ap.ContentType, Field.Store.YES, Field.Index.NO));
                    if (File.Exists(Path.Combine(lucenepath, ap.Raw_FileName)) == false)
                    {
                        try
                        {
                            File.Copy(Path.Combine(dir, ap.Raw_FileName), Path.Combine(lucenepath, ap.Raw_FileName), false);
                        }
                        catch (Exception ex) { }
                    }
                    writer.AddDocument(images);
                }                
            }
            else
            {
                doc.Add(new Field("AdPhotos", "_NULL_", Field.Store.YES, Field.Index.NOT_ANALYZED));
            }
            // add entry to index
            writer.AddDocument(doc);
        }

        /// <summary>
        /// Delete all ad/lucene images and remove from lucene
        /// </summary>
        /// <param name="ad"></param>
        public static void DeletePhotosFromLuceneIndex(int adId, string stringId, IEnumerable<ClassifiedAdPhoto> photos)
        {
            var lucenerecord = SearchEngineManager.GetClassifiedAdWithDetails(adId);
            // delete photos
            var dir = HostingEnvironment.MapPath("~/Photos/" + stringId.Substring(2, 4) + "/" + stringId.Substring(0, 4));
            var lucenepath = Path.Combine(dir, "lucene");
            if (photos != null)
            {
                foreach (var pho in photos)
                {
                    try
                    {
                        // Delete Regular
                        if (File.Exists(Path.Combine(dir, pho.AdDetails_FileName)))
                            File.Delete(Path.Combine(dir, pho.AdDetails_FileName));
                        if (File.Exists(Path.Combine(dir, pho.AdList_FileName)))
                            File.Delete(Path.Combine(dir, pho.AdList_FileName));
                        if (File.Exists(Path.Combine(dir, pho.Raw_FileName)))
                            File.Delete(Path.Combine(dir, pho.Raw_FileName));
                    }
                    catch (Exception) { }
                }
            }
            if(lucenerecord != null && lucenerecord.Photos != null)
            foreach (var pho in lucenerecord.Photos)
            {
                // Delete Lucene
                if (File.Exists(Path.Combine(lucenepath, pho.AdDetails_FileName)))
                    File.Delete(Path.Combine(lucenepath, pho.AdDetails_FileName));
                if (File.Exists(Path.Combine(lucenepath, pho.AdList_FileName)))
                    File.Delete(Path.Combine(lucenepath, pho.AdList_FileName));
                if (File.Exists(Path.Combine(lucenepath, pho.Raw_FileName)))
                    File.Delete(Path.Combine(lucenepath, pho.Raw_FileName));                
            }

            // Delete temp folder
            PhotoFileManager.DeleteTempPhotos(adId, stringId);
        }

        // Bulk Add
        public static void AdminCreateLuceneIndex(IEnumerable<ClassifiedAdLucene> data)
        {
            // init lucene
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(_write, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add new index entry
                // add new index entry
                Document doc = null;
                var ManagerSeo = new SeoManager();
                // add data to lucene search index (replaces older entry if any)
                foreach (var sampleData in data)
                {
                    _createLuceneIndex(sampleData, writer, ManagerSeo, doc);
                };
            }
        }

        public static void AddUpdateLuceneIndex(ClassifiedAdLucene sampleData)
        {
            var oldlucenerecord = SearchEngineManager.GetClassifiedAdWithDetails(sampleData.Id);
            var dir = HostingEnvironment.MapPath("~/Photos/" + sampleData.StringId.Substring(2, 4) + "/" + sampleData.StringId.Substring(0, 4));
            var lucenepath = Path.Combine(dir, "lucene");
            // init lucene
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(_write, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var searchQuery = new TermQuery(new Term("Id", sampleData.Id.ToString()));
                writer.DeleteDocuments(searchQuery);
                // Add
                Document doc = null;
                var ManagerSeo = new SeoManager();
                _createLuceneIndex(sampleData, writer, ManagerSeo, doc);                
            }
            var newlucenerecord = SearchEngineManager.GetClassifiedAdWithDetails(sampleData.Id);
            // Compare photos for changes and remove unused
            if (oldlucenerecord != null && newlucenerecord != null)
            {
                var photostodelete = oldlucenerecord.Photos.Except(newlucenerecord.Photos, new PhotoComparer());
                foreach (var pho in photostodelete)
                {
                    // Delete Lucene
                    if (File.Exists(Path.Combine(lucenepath, pho.AdDetails_FileName)))
                        File.Delete(Path.Combine(lucenepath, pho.AdDetails_FileName));
                    if (File.Exists(Path.Combine(lucenepath, pho.AdList_FileName)))
                        File.Delete(Path.Combine(lucenepath, pho.AdList_FileName));
                    if (File.Exists(Path.Combine(lucenepath, pho.Raw_FileName)))
                        File.Delete(Path.Combine(lucenepath, pho.Raw_FileName));
                }
            }
        }

        public static void AddUpdateLuceneIndex(ClassifiedAd sampleData)
        {
            AddUpdateLuceneIndex(Mapper.Map<ClassifiedAdLucene>(sampleData));
        }

        public static void Optimize()
        {
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            using (var writer = new IndexWriter(_write, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }
        
        // Map to ClassifiedAdList
        public static T _mapLuceneDocumentToData<T>(Document doc)
        {
            return Mapper.Map<T>(doc);
        }

        /// <summary>
        /// Remove from the index
        /// </summary>
        /// <param name="record_id"></param>
        public static void ClearLuceneIndexRecord(int record_id, ICollection<ClassifiedAdPhoto> photos = null)
        {
            // init lucene
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(_write, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var t = new List<TermQuery>() { new TermQuery(new Term("Id", record_id.ToString())) };
                if(photos != null)
                {
                    t.AddRange(from pho in photos
                               select new TermQuery(new Term("AdPhotoLocatorId", string.Format("{0}-{1}", record_id, pho.AdDetails_FileName))));
                    t.AddRange(from pho in photos
                               select new TermQuery(new Term("AdPhotoLocatorId", string.Format("{0}-{1}", record_id, pho.AdList_FileName))));
                    t.AddRange(from pho in photos
                               select new TermQuery(new Term("AdPhotoLocatorId", string.Format("{0}-{1}", record_id, pho.Raw_FileName))));
                }
                writer.DeleteDocuments(t.ToArray());
                writer.Commit();
            }
        }

        /// <summary>
        /// Remove All
        /// </summary>
        public static void ClearAllLuceneIndexRecords()
        {
            // init lucene
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(_write, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteAll();
                writer.Commit();
            }
        }

        public static IEnumerable<T> _mapLuceneToDataListRandom<T>(IEnumerable<ScoreDoc> hits,
            IndexSearcher searcher)
        {
            Random rnd = new Random();
            return hits.Select(hit => _mapLuceneDocumentToData<T>(searcher.Doc(hit.Doc))).OrderBy(x => rnd.Next()).Take(4).ToList();
        }

        public static IEnumerable<T> _mapLuceneToDataList<T>(IEnumerable<Document> hits)
        {
            return hits.Select(_mapLuceneDocumentToData<T>).ToList();
        }

        public static IEnumerable<T> _mapLuceneToDataList<T>(IEnumerable<ScoreDoc> hits,
            IndexSearcher searcher)
        {
            return hits.Select(hit => _mapLuceneDocumentToData<T>(searcher.Doc(hit.Doc))).ToList();
        }

        public static Query parseQuery(string searchQuery, QueryParser parser)
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

        public static FuzzyQuery parseFuzzyQuery(string searchQuery, QueryParser parser)
        {
            var query = new FuzzyQuery(new Term("ContentText", searchQuery), 0.9f);
            return query;
        }
    }
}