using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Trinbago_MVC5.Models;
using PagedList;
using Lucene.Net.Search;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;

namespace Trinbago_MVC5.Controllers
{
    public class SearchEngine : Manager
    {
        public IPagedList<ClassifiedAdList> GetClassifiedAdListSearchEngine(int stringIdCategory, string stringIdSubCategory, string searchString = null, int? pageNumber = 1, string searchCategory = "ALL",
            string searchOnlyOption = "All Ads", string minPrice = null, string maxPrice = null, string minMile = null, string maxMile = null, string modelName = null, string minYear = null, string maxYear = null,
            string modEngineSize = null, string modelMake = null, string modelBodyType = null, string modelDrivetrain = null, string modelTransmission = null, string modelCondition = null, string modelColour = null,
            string modelJobType = null, string modelSalaryInfo = null, string modelBedrooms = null, string modelBathrooms = null, string modelFurnished = null, string CountryId = null, string RegionId = null,
            string minSize = null, string maxSize = null, string modelBreed = null, string modelSpecies = null, string minAge = null, string maxAge = null, string ageType = null)
        {
            // LUCENE
            // validation            
            // Setup query
            BooleanQuery bq = new BooleanQuery();
            var sortBy = new Sort();
            sortBy.SetSort(new SortField("EditTimeStampTicks",SortField.LONG,true), SortField.FIELD_SCORE);
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

            // Get only Open ads
            bq.Add(new TermQuery(new Lucene.Net.Index.Term("Status", "0")), Occur.MUST);

            if (!String.IsNullOrEmpty(searchString))
            {
                bq.Add(LuceneSearch.parseQuery(searchString, new MultiFieldQueryParser
                    (Lucene.Net.Util.Version.LUCENE_30, new[] { 
                        "Title", "HtmlFreeDescription", "Country", "Region",
                        "Make", "Model", "Mileage", "Year", "Engine Size", "Condition", "Colour", "Fuel Type", "Transmission", "Drivetrain", "Body Type",
                        "Company Name", "Job Type",
                        "Species", "Breed", "Gender", "Age", "Pet's Name" }, analyzer))
                        , Occur.MUST);
                sortBy = Sort.RELEVANCE;
            }

            if (searchOnlyOption.Equals("All Ads"))
            {
                if (stringIdCategory != -100)
                {
                    if (searchCategory.Equals("ALL"))
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("CategoryId", stringIdCategory.ToString())), Occur.MUST);
                    }
                    else
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("CategoryId", stringIdCategory.ToString())), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("AdType", searchCategory)), Occur.MUST);
                    }
                }
                else if (!String.IsNullOrEmpty(stringIdSubCategory))
                {
                    if (searchCategory.Equals("ALL"))
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("SubCategoryStringId", stringIdSubCategory)), Occur.MUST);
                    }
                    else
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("SubCategoryStringId", stringIdSubCategory)), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("AdType", searchCategory)), Occur.MUST);
                    }
                }
                else
                {
                    if (!searchCategory.Equals("ALL"))
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("AdType", searchCategory)), Occur.MUST);
                    }
                }
            }
            else if (searchOnlyOption.Equals("Top Ads"))
            {
                if (stringIdCategory != -100)
                {
                    if (searchCategory.Equals("ALL"))
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("CategoryId", stringIdCategory.ToString())), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("TopAdStatus", "true")), Occur.MUST);
                    }
                    else
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("CategoryId", stringIdCategory.ToString())), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("TopAdStatus", "true")), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("AdType", searchCategory)), Occur.MUST);
                    }
                }
                else if (!String.IsNullOrEmpty(stringIdSubCategory))
                {
                    if (searchCategory.Equals("ALL"))
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("SubCategoryStringId", stringIdSubCategory)), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("TopAdStatus", "true")), Occur.MUST);
                    }
                    else
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("SubCategoryStringId", stringIdSubCategory)), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("TopAdStatus", "true")), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("AdType", searchCategory)), Occur.MUST);
                    }
                }
                else
                {
                    if (!searchCategory.Equals("ALL"))
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("TopAdStatus", "true")), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("AdType", searchCategory)), Occur.MUST);
                    }
                }
            }
            else if (searchOnlyOption.Equals("Urgent Ads"))
            {
                if (stringIdCategory != -100)
                {
                    if (searchCategory.Equals("ALL"))
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("CategoryId", stringIdCategory.ToString())), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("UrgentAdStatus", "true")), Occur.MUST);
                    }
                    else
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("CategoryId", stringIdCategory.ToString())), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("UrgentAdStatus", "true")), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("AdType", searchCategory)), Occur.MUST);
                    }
                }
                else if (!String.IsNullOrEmpty(stringIdSubCategory))
                {
                    if (searchCategory.Equals("ALL"))
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("SubCategoryStringId", stringIdSubCategory)), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("UrgentAdStatus", "true")), Occur.MUST);
                    }
                    else
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("SubCategoryStringId", stringIdSubCategory)), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("UrgentAdStatus", "true")), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("AdType", searchCategory)), Occur.MUST);
                    }
                }
                else
                {
                    if (!searchCategory.Equals("ALL"))
                    {
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("UrgentAdStatus", "true")), Occur.MUST);
                        bq.Add(new TermQuery(new Lucene.Net.Index.Term("AdType", searchCategory)), Occur.MUST);
                    }
                }
            }

            //
            // FILTERS
            //max /min price
            if (minPrice != null)            
                if (minPrice != "" && maxPrice == "")
                    bq.Add(NumericRangeQuery.NewIntRange("Price", Int32.Parse(minPrice.Replace(",", "")), Int32.Parse(minPrice.Replace(",", "")), true, true), Occur.MUST);

            if (maxPrice != null)
                if (maxPrice != "" && minPrice == "")
                    bq.Add(NumericRangeQuery.NewIntRange("Price", Int32.Parse(maxPrice.Replace(",", "")), Int32.Parse(maxPrice.Replace(",", "")), true, true), Occur.MUST);

            if (maxPrice != null && minPrice != null)
                if (maxPrice != "" && minPrice != "")
                    bq.Add(NumericRangeQuery.NewIntRange("Price", Int32.Parse(minPrice.Replace(",", "")), Int32.Parse(maxPrice.Replace(",", "")), true, true), Occur.MUST);
            

            //location
            if (!String.IsNullOrEmpty(CountryId))
                bq.Add(new TermQuery(new Lucene.Net.Index.Term("CountryId", CountryId)), Occur.MUST);
            

            if (!String.IsNullOrEmpty(RegionId))
                bq.Add(new TermQuery(new Lucene.Net.Index.Term("RegionId", RegionId)), Occur.MUST);            

            //model make
            if (!String.IsNullOrEmpty(modelMake))
                bq.Add(new TermQuery(new Lucene.Net.Index.Term("Make", modelMake)), Occur.MUST);

            //max / min mileage
            if(minMile != null)
                if (minMile != "" && maxMile == "")
                    bq.Add(NumericRangeQuery.NewIntRange("Mileage", Int32.Parse(minMile.Replace(",", "")), Int32.Parse(minMile.Replace(",", "")), true, true), Occur.MUST);

            if(maxMile != null)
                if (maxMile != "" && minMile == "")
                    bq.Add(NumericRangeQuery.NewIntRange("Mileage", Int32.Parse(maxMile.Replace(",", "")), Int32.Parse(maxMile.Replace(",", "")), true, true), Occur.MUST);

            if(maxMile != null && minMile != null)
                if (maxMile != "" && minMile != "")
                    bq.Add(NumericRangeQuery.NewIntRange("Mileage", Int32.Parse(minMile.Replace(",","")), Int32.Parse(maxMile.Replace(",","")), true, true), Occur.MUST);
            
            //model name
            if (!String.IsNullOrEmpty(modelName))            
                bq.Add(new FuzzyQuery(new Lucene.Net.Index.Term("Model", modelName)), Occur.MUST);
            

            //max / min year
            if(minYear != null && maxYear == null)
                if (minYear != "" && maxYear == "")
                    bq.Add(NumericRangeQuery.NewIntRange("Year", Int32.Parse(minYear), Int32.Parse(minYear), true, true), Occur.MUST);

            if(maxYear != null && minYear == null)
                if (maxYear != "" && minYear == "")
                    bq.Add(NumericRangeQuery.NewIntRange("Year", Int32.Parse(maxYear), Int32.Parse(maxYear), true, true), Occur.MUST);

            if(maxYear != null && minYear != null)
                if (maxYear != "" && minYear != "")
                    bq.Add(NumericRangeQuery.NewIntRange("Year", Int32.Parse(minYear), Int32.Parse(maxYear), true, true), Occur.MUST);

            //model body type
            if (!String.IsNullOrEmpty(modelBodyType))
                bq.Add(new TermQuery(new Lucene.Net.Index.Term("Body Type", modelBodyType)), Occur.MUST);
            
            //model drivetrain
            if (!String.IsNullOrEmpty(modelDrivetrain))
                bq.Add(new TermQuery(new Lucene.Net.Index.Term("Drivetrain", modelDrivetrain)), Occur.MUST);

            //model transmission
            if (!String.IsNullOrEmpty(modelTransmission))
                bq.Add(new TermQuery(new Lucene.Net.Index.Term("Transmission", modelTransmission)), Occur.MUST);

            //model condition
            if (!String.IsNullOrEmpty(modelCondition))
                bq.Add(new TermQuery(new Lucene.Net.Index.Term("Condition", modelCondition)), Occur.MUST);
            
            //model colour
            if (!String.IsNullOrEmpty(modelColour))
                bq.Add(new TermQuery(new Lucene.Net.Index.Term("Colour", modelColour)), Occur.MUST);
            
            //model engine size
            if (!String.IsNullOrEmpty(modEngineSize))
                bq.Add(new TermQuery(new Lucene.Net.Index.Term("Engine Size", modEngineSize)), Occur.MUST);

            //------JOBS---------
            //model job type
            if (!String.IsNullOrEmpty(modelJobType))
                bq.Add(new TermQuery(new Lucene.Net.Index.Term("Job Type", modelJobType)), Occur.MUST);
            
            //model salary type
            if (!String.IsNullOrEmpty(modelSalaryInfo))
                bq.Add(new TermQuery(new Lucene.Net.Index.Term("Salary Type", modelSalaryInfo)), Occur.MUST);
            
            //-----REAL ESTATE----
            //model min/max size
            if(minSize != null)
                if (minSize != "" && maxSize == "")
                    bq.Add(NumericRangeQuery.NewIntRange("Size", Int32.Parse(minSize), Int32.Parse(minSize), true, true), Occur.MUST);

            if(maxSize != null)
                if (maxSize != "" && minSize == "")
                    bq.Add(NumericRangeQuery.NewIntRange("Size", Int32.Parse(maxSize), Int32.Parse(maxSize), true, true), Occur.MUST);

            if(maxSize != null && minSize != null)
                if (maxSize != "" & minSize != "")
                    bq.Add(NumericRangeQuery.NewIntRange("Size", Int32.Parse(minSize), Int32.Parse(maxSize), true, true), Occur.MUST);

            //model bedrooms
            if(modelBathrooms != null)
                if (modelBedrooms != "")
                    bq.Add(new TermQuery(new Lucene.Net.Index.Term("Bedrooms", modelBedrooms)), Occur.MUST);
            
            //model bathrooms
            if(modelBathrooms != null)
                if (modelBathrooms != "")
                    bq.Add(new TermQuery(new Lucene.Net.Index.Term("Bathrooms", modelBathrooms)), Occur.MUST);

            //model furnished
            if(modelFurnished != null)
                if (modelFurnished != "")
                    bq.Add(new TermQuery(new Lucene.Net.Index.Term("Furnished", modelFurnished)), Occur.MUST);

            //-----Pets--------
            ///////////////////
            if (modelSpecies != null)
                if(modelSpecies != "")
                    bq.Add(new TermQuery(new Lucene.Net.Index.Term("Species", modelSpecies)), Occur.MUST);

            if(modelBreed != null)
                if (modelBreed != "")
                    bq.Add(new TermQuery(new Lucene.Net.Index.Term("Breed", modelBreed)), Occur.MUST);
            
            //model min/max age
            if (minAge != null && maxAge == null)
                if (minAge != "" && maxAge == "")
                    bq.Add(new TermRangeQuery("Age", minAge, minAge, true, true), Occur.MUST);
            
            if (maxAge != null && minAge == null)
                if (maxAge != "" && minAge == "")
                    bq.Add(new TermRangeQuery("Age", maxAge, maxAge, true, true), Occur.MUST);
            
            if(maxAge != null && minAge != null)
                if (maxAge != "" && minAge != "")
                    bq.Add(new TermRangeQuery("Age", minAge, maxAge, true, true), Occur.MUST);

            // set up lucene searcher
            using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
            {
                var hits_limit = 1000;
                var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                var results = LuceneSearch._mapLuceneToDataList(hits, searcher);
                analyzer.Close();
                searcher.Dispose();

                var temppaged = new PagedList<ClassifiedAdList>(results, pageNumber.Value, RecordsPerPage.recordsPerPage);

                return temppaged;
            }
        }

        public IPagedList<PUserClassifiedAdList> GetPUserClassifiedAdListSearchEngine(int stringIdCategory, string stringIdSubCategory, string searchString = null, int? pageNumber = 1, string searchCategory = "ALL",
            string searchOnlyOption = "All Ads", string minPrice = null, string maxPrice = null, string minMile = null, string maxMile = null, string modelName = null, string minYear = null, string maxYear = null,
            string modEngineSize = null, string modelMake = null, string modelBodyType = null, string modelDrivetrain = null, string modelTransmission = null, string modelCondition = null, string modelColour = null,
            string modelJobType = null, string modelSalaryInfo = null, string modelBedrooms = null, string modelBathrooms = null, string modelFurnished = null, string CountryId = null, string RegionId = null,
            string minSize = null, string maxSize = null, string modelBreed = null, string modelSpecies = null, string minAge = null, string maxAge = null, string ageType = null)
        {
            IQueryable<ClassifiedAd> items = null;
            // spin up a new context per thread
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // check if string id has value
                var s = newthreadcontext.SubCategoryDB.SingleOrDefault(x => x.stringId.Equals(stringIdSubCategory));
                if (searchOnlyOption.Equals("All Ads"))
                {
                    if (stringIdCategory != -100)
                    {
                        if (searchCategory.Equals("ALL"))
                        {
                            items = newthreadcontext.ClassifiedDB.Include("Category").Include("AdInfo")
                             .Include("AdPhotos").Where(x => x.Status == 0 && x.Category.Id.Equals(stringIdCategory));
                        }
                        else
                        {
                            items = newthreadcontext.ClassifiedDB.Include("Category").Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.AdType.Equals(searchCategory) && x.Category.Id.Equals(stringIdCategory));
                        }
                    }
                    else if (!String.IsNullOrEmpty(stringIdSubCategory))
                    {
                        if (searchCategory.Equals("ALL"))
                        {
                            items = newthreadcontext.ClassifiedDB.Include("Category").Include("SubCategory").Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.SubCategory.stringId.Equals(stringIdSubCategory));
                        }
                        else
                        {
                            items = newthreadcontext.ClassifiedDB.Include("Category").Include("SubCategory").Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.AdType.Equals(searchCategory) && x.SubCategory.stringId.Equals(stringIdSubCategory));
                        }
                    }
                    else
                    {
                        if (searchCategory.Equals("ALL"))
                        {
                            items = newthreadcontext.ClassifiedDB.Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0);
                        }
                        else
                        {
                            items = newthreadcontext.ClassifiedDB.Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.AdType.Equals(searchCategory));
                        }
                    }
                }
                else if (searchOnlyOption.Equals("Top Ads"))
                {
                    if (stringIdCategory != -100)
                    {
                        if (searchCategory.Equals("ALL"))
                        {
                            items = newthreadcontext.ClassifiedDB.Include("Category").Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.Category.Id.Equals(stringIdCategory) && x.TopAdStatus == true);
                        }
                        else
                        {
                            items = newthreadcontext.ClassifiedDB.Include("Category").Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.AdType.Equals(searchCategory) && x.Category.Id.Equals(stringIdCategory) && x.TopAdStatus == true);
                        }
                    }
                    else if (!String.IsNullOrEmpty(stringIdSubCategory))
                    {
                        if (searchCategory.Equals("ALL"))
                        {
                            items = newthreadcontext.ClassifiedDB.Include("Category").Include("SubCategory").Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.SubCategory.stringId.Equals(stringIdSubCategory) && x.TopAdStatus == true);
                        }
                        else
                        {
                            items = newthreadcontext.ClassifiedDB.Include("Category").Include("SubCategory").Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.AdType.Equals(searchCategory) && x.SubCategory.stringId.Equals(stringIdSubCategory) && x.TopAdStatus == true);
                        }
                    }
                    else
                    {
                        if (searchCategory.Equals("ALL"))
                        {
                            items = newthreadcontext.ClassifiedDB.Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.TopAdStatus == true);
                        }
                        else
                        {
                            items = newthreadcontext.ClassifiedDB.Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.AdType.Equals(searchCategory) && x.TopAdStatus == true);
                        }
                    }
                }
                else if (searchOnlyOption.Equals("Urgent Ads"))
                {
                    if (stringIdCategory != -100)
                    {
                        if (searchCategory.Equals("ALL"))
                        {
                            items = newthreadcontext.ClassifiedDB.Include("Category").Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.Category.Id.Equals(stringIdCategory) && x.UrgentAdStatus == true);
                        }
                        else
                        {
                            items = newthreadcontext.ClassifiedDB.Include("Category").Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.AdType.Equals(searchCategory) && x.Category.Id.Equals(stringIdCategory) && x.UrgentAdStatus == true);
                        }
                    }
                    else if (!String.IsNullOrEmpty(stringIdSubCategory))
                    {
                        if (searchCategory.Equals("ALL"))
                        {
                            items = newthreadcontext.ClassifiedDB.Include("Category").Include("SubCategory").Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.SubCategory.stringId.Equals(stringIdSubCategory) && x.UrgentAdStatus == true);
                        }
                        else
                        {
                            items = newthreadcontext.ClassifiedDB.Include("Category").Include("SubCategory").Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.AdType.Equals(searchCategory) && x.SubCategory.stringId.Equals(stringIdSubCategory) && x.UrgentAdStatus == true);
                        }
                    }
                    else
                    {
                        if (searchCategory.Equals("ALL"))
                        {
                            items = newthreadcontext.ClassifiedDB.Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.UrgentAdStatus == true);
                        }
                        else
                        {
                            items = newthreadcontext.ClassifiedDB.Include("AdInfo")
                            .Include("AdPhotos").Where(x => x.Status == 0 && x.AdType.Equals(searchCategory) && x.UrgentAdStatus == true);
                        }

                    }
                }

                if (!String.IsNullOrEmpty(searchString))
                {
                    var temp = items.Where(x => x.Title.Contains(searchString)
                        || x.Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Make")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Mileage")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Model")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Year")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Body Type")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Drivetrain")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Transmission")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Condition")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Colour")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Engine Size")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Job Type")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Job Type")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Salary Type")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Size")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Bedrooms")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Bathrooms")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Furnished")).Description.Contains(searchString.Contains("Furnished") ? "Yes" : null)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Breed")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Species")).Description.Contains(searchString)
                        || x.AdInfo.FirstOrDefault(m => m.Name.Equals("Age")).Description.Contains(searchString)
                        );
                    if (!temp.Any())
                    {
                        var temp2 = items.Where(x => x.Category.Name.Contains(searchString));
                        if (temp2.Any())
                        {
                            items = temp2;
                        }
                        else
                        {
                            var temp3 = items.Where(x => x.SubCategory.Name.Contains(searchString));
                            if (temp3.Any())
                            {
                                items = temp2;
                            }
                            else
                            {
                                items = temp;
                            }
                        }
                    }
                }
                else
                {
                    items = items.Where(x => x.FeaturedAdStatus == false);
                }
                //
                // FILTERS
                //max /min price
                if (minPrice != null)
                {
                    int min = Convert.ToInt32(minPrice == "" ? null : minPrice.Replace(",", ""));
                    if (min != 0)
                        items = items.Where(x => x.Price >= min);
                }

                if (maxPrice != null)
                {
                    int max = Convert.ToInt32(maxPrice == "" ? null : maxPrice.Replace(",", ""));
                    if (max != 0)
                        items = items.Where(x => x.Price <= max);
                }

                //location
                if (!String.IsNullOrEmpty(CountryId))
                {
                    int cid = Convert.ToInt32(CountryId == "" ? null : CountryId);
                    if (cid != 0)
                        items = items.Where(x => x.Country.Id == cid);
                }

                if (!String.IsNullOrEmpty(RegionId))
                {
                    int rid = Convert.ToInt32(RegionId == "" ? null : RegionId);
                    if (rid != 0)
                        items = items.Where(x => x.Region.Id == rid);
                }


                //model make
                if (!String.IsNullOrEmpty(modelMake))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Make")).Description.Equals(modelMake));

                //max / min mileage
                if (minMile != null)
                {
                    var min = Convert.ToInt32(minMile == "" ? null : minMile.Replace(",", ""));
                    if (min != 0)
                        items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Mileage")).IntDescription >= min);
                }
                if (maxMile != null)
                {
                    var max = Convert.ToInt32(maxMile == "" ? null : maxMile.Replace(",", ""));
                    if (max != 0)
                        items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Mileage")).IntDescription <= max);
                }

                //model name
                if (!String.IsNullOrEmpty(modelName))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Model")).Description.Contains(modelName));

                //max / min year
                if (minYear != null)
                {
                    var min = Convert.ToInt32(minYear == "" ? null : minYear);
                    if (min != 0)
                        items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Year")).IntDescription >= min);
                }
                if (maxYear != null)
                {
                    var max = Convert.ToInt32(maxYear == "" ? null : maxYear);
                    if (max != 0)
                        items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Year")).IntDescription <= max);
                }

                //model body type
                if (!String.IsNullOrEmpty(modelBodyType))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Body Type")).Description.Equals(modelBodyType));

                //model drivetrain
                if (!String.IsNullOrEmpty(modelDrivetrain))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Drivetrain")).Description.Equals(modelDrivetrain));

                //model transmission
                if (!String.IsNullOrEmpty(modelTransmission))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Transmission")).Description.Equals(modelTransmission));

                //model condition
                if (!String.IsNullOrEmpty(modelCondition))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Condition")).Description.Equals(modelCondition));

                //model colour
                if (!String.IsNullOrEmpty(modelColour))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Colour")).Description.Contains(modelColour));

                //model engine size
                if (!String.IsNullOrEmpty(modEngineSize))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Engine Size")).Description.Equals(modEngineSize.Replace(" ", "")));

                //------JOBS---------
                //model job type
                if (!String.IsNullOrEmpty(modelJobType))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Job Type")).Description.Equals(modelJobType));

                //model salary type
                if (!String.IsNullOrEmpty(modelSalaryInfo))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Salary Type")).Description.Equals(modelSalaryInfo));

                //-----REAL ESTATE----
                //model min/max size
                if (!String.IsNullOrEmpty(minSize))
                {
                    var min = Convert.ToInt32(minSize == "" ? null : minSize);
                    if (min != 0)
                        items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Size")).IntDescription >= min);
                }
                if (!string.IsNullOrEmpty(maxSize))
                {
                    var max = Convert.ToInt32(maxSize == "" ? null : maxSize);
                    if (max != 0)
                        items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Size")).IntDescription <= max);
                }

                //model bedrooms
                if (!String.IsNullOrEmpty(modelBedrooms))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Bedrooms")).Description.Equals(modelBedrooms));

                //model bathrooms
                if (!String.IsNullOrEmpty(modelBathrooms))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Bathrooms")).Description.Equals(modelBathrooms));

                //model furnished
                if (!String.IsNullOrEmpty(modelFurnished))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Furnished")).Description.Equals(modelFurnished));

                //-----Pets--------
                ///////////////////
                if (!String.IsNullOrEmpty(modelSpecies))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Species")).Description.Equals(modelSpecies));

                if (!String.IsNullOrEmpty(modelBreed))
                    items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Breed")).Description.Equals(modelBreed));



                //model min/max age
                if (!String.IsNullOrEmpty(minAge))
                {
                    var min = Convert.ToInt32(minAge == "" ? null : minAge);
                    if (min != 0)
                        items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Age") && m.Description.Contains(ageType)).IntDescription >= min);
                }
                if (!string.IsNullOrEmpty(maxAge))
                {
                    var max = Convert.ToInt32(maxAge == "" ? null : maxAge);
                    if (max != 0)
                        items = items.Where(x => x.AdInfo.FirstOrDefault(m => m.Name.Equals("Age") && m.Description.Contains(ageType)).IntDescription <= max);
                }


                if (items.Any())
                {
                    items = items.OrderBy(x => x.UrgentAdStatus ? false : true);
                    items = items.OrderBy(x => x.TopAdStatus ? false : true);

                    if (items.Any())
                    {
                        var config = new MapperConfiguration(r =>
                        {
                            r.CreateMap<ClassifiedAd, PUserClassifiedAdList>()
                                .ForMember(dest => dest.AdPhoto, opt => opt.MapFrom(src => src.AdPhotos.SingleOrDefault(x => x.SetThumbnail == true)))
                                .AfterMap((src, dest) => dest.AdInfo = dest.AdInfo.GroupBy(x => x.Name).Select(g => g.First()).ToList());
                            r.CreateMap<Country, CountryBase>();
                            r.CreateMap<Info, InfoForm>();
                            r.CreateMap<Photo, PhotoBase>();

                        });
                        IMapper mapper = config.CreateMapper();

                        var toreturn = new PagedList<PUserClassifiedAdList>(mapper.Map<IEnumerable<PUserClassifiedAdList>>(items), pageNumber.Value, RecordsPerPage.recordsPerPage);
                        return toreturn;
                    }
                }
                ICollection<PUserClassifiedAdList> none = new List<PUserClassifiedAdList>();
                return none.ToPagedList(pageNumber.Value, RecordsPerPage.recordsPerPage);
            }
        }

        internal bool isValidCatId(string searchBarId)
        {
            using (ApplicationDbContext newtheadcontext = new ApplicationDbContext())
            {
                int id;
                Int32.TryParse(searchBarId, out id);
                var obj = newtheadcontext.CategoryDB.SingleOrDefault(x => x.Id == id);
                return (obj == null) ? false : true;
            }
        }

        internal bool isValidSubCatId(string searchBarId)
        {
            using (ApplicationDbContext newtheadcontext = new ApplicationDbContext())
            {
                var obj = newtheadcontext.SubCategoryDB.SingleOrDefault(x => x.stringId.Equals(searchBarId));
                return (obj == null) ? false : true;
            }
        }
    }
}