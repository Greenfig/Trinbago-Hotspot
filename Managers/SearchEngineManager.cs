using System.Linq;
using PagedList;
using Lucene.Net.Search;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Trinbago_MVC5.Extensions;
using System.Collections.Generic;
using Lucene.Net.Index;
using StackExchange.Profiling;
using Trinbago_MVC5.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Areas.User.Models;
using System;

namespace Trinbago_MVC5.Controllers
{
    public static class SearchEngineManager
    {
        public static IPagedList<ClassifiedAdList> GetClassifiedAdListSearchEngine(int? catId, int subCatId, string searchString = null, int? pageNumber = 1, string searchCategory = "ALL",
            string searchOnlyOption = "All Ads", string minPrice = "", string maxPrice = "", string minMile = "", string maxMile = "", string modelName = "", string minYear = "", string maxYear = "",
            string modEngineSize = "", string modelMake = "", string modelBodyType = "", string modelDrivetrain = "", string modelTransmission = "", string modelCondition = "", string modelColour = "",
            string modelJobType = "", string modelSalaryInfo = "", string modelRentalType = "", string modelBedrooms = "", string modelBathrooms = "", string modelFurnished = "", int? CountryId = null, int? RegionId = null,
            string minSize = "", string maxSize = "", string modelSpecies = "", string minAge = "", string maxAge = "", string ageType = "")
        {
#if DEBUG
            var profiler = MiniProfiler.Current;
            using (profiler.Step("lucene search"))
            {
#endif
                // LUCENE           
                // Setup query
                BooleanQuery bq = new BooleanQuery();
                var sortBy = new Sort();
                sortBy.SetSort(
                    new SortField("AdPhotos", SortField.STRING), 
                    new SortField("AdType", SortField.STRING),
                    new SortField("TopAdStatus", SortField.STRING, true),
                    new SortField("TimeStampTicks", SortField.LONG, true),
                    SortField.FIELD_SCORE);
                using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                {

                    // Specify ads
                    bq.Add(NumericRangeQuery.NewIntRange("Status", 0, 2, true, true), Occur.MUST);

                    if (!string.IsNullOrEmpty(searchString))
                    {
                        bq.Add(LuceneSearch.parseQuery(searchString, new MultiFieldQueryParser
                            (Lucene.Net.Util.Version.LUCENE_30, new[] {
                        "Title", "HtmlFreeDescription", "Country", "Region",
                        "Make", "Model", "Mileage", "Year", "Engine Size", "Condition", "Colour", "Fuel Type", "Transmission", "Drivetrain", "Body Type",
                        "Company Name", "Job Type",
                        "Species", "Age", "Pet's Name" }, analyzer))
                                , Occur.MUST);
                        sortBy = Sort.RELEVANCE;
                    }

                    // Show all ads in a Subcategory
                    if (subCatId > 0)
                    {
                        bq.Add(new TermQuery(new Term("SubCategoryId", subCatId.ToString())), Occur.MUST);                        
                    }
                    // Show all ads in a Category
                    else if (catId.HasValue && catId > 0)
                    {
                        bq.Add(new TermQuery(new Term("CategoryId", catId.Value.ToString())), Occur.MUST);
                        if (searchString == null)
                        {
                            // Filter out livestock
                            bq.Add(new TermQuery(new Term("SubCategoryName", "Livestock")), Occur.MUST_NOT);
                        }
                    }
                    // Show all ads
                    else
                    {
                        // If search query is null use job and livestock filter
                        if (searchString == null)
                        {
                            // Filter out jobs
                            bq.Add(new TermQuery(new Term("CategoryName", "Jobs")), Occur.MUST_NOT);
                            // Filter out livestock
                            bq.Add(new TermQuery(new Term("SubCategoryName", "Livestock")), Occur.MUST_NOT);
                        }
                    }
                    
                    // Filter All & Urgent Ads
                    if (!searchCategory.Equals("ALL"))
                    {
                        bq.Add(new TermQuery(new Term("AdType", searchCategory)), Occur.MUST);
                    }

                    if (searchOnlyOption.Equals("Urgent Ads"))
                    {
                        bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                    }

                    //
                    // FILTERS
                    //max /min price

                    int _minPrice = 0;
                    int _maxPrice = 0;
                    int.TryParse(minPrice.Replace(",", ""), out _minPrice);
                    int.TryParse(maxPrice.Replace(",", ""), out _maxPrice);
                    if (_minPrice != 0 || _maxPrice != 0)
                    {
                        // If min is set get all ads >= min
                        if (_minPrice > 0 && _maxPrice == 0)
                            bq.Add(NumericRangeQuery.NewIntRange("Price", _minPrice, null, true, true), Occur.MUST);
                        // If max is set get all ads <=max
                        else if (_minPrice == 0 && _maxPrice > 0)
                            bq.Add(NumericRangeQuery.NewIntRange("Price", null, _maxPrice, true, true), Occur.MUST);
                        // If max and min is set get all ads >= min && <= max
                        else if (_minPrice > 0 && _maxPrice > 0)
                            bq.Add(NumericRangeQuery.NewIntRange("Price", _minPrice, _maxPrice, true, true), Occur.MUST);
                    }

                    //location
                    if (RegionId.HasValue && RegionId > 0)
                        bq.Add(NumericRangeQuery.NewIntRange("RegionId", RegionId.Value, RegionId.Value, true, true), Occur.MUST);
                    else if (CountryId.HasValue && CountryId > 0)
                        bq.Add(NumericRangeQuery.NewIntRange("CountryId", CountryId.Value, CountryId.Value, true, true), Occur.MUST);

                    //model make
                    if (!string.IsNullOrEmpty(modelMake))
                        bq.Add(new TermQuery(new Term("Make", modelMake)), Occur.MUST);

                    //max / min mileage
                    int _minMile = 0;
                    int _maxMile = 0;
                    int.TryParse(minMile.Replace(",", ""), out _minMile);
                    int.TryParse(maxMile.Replace(",", ""), out _maxMile);
                    if (_minMile != 0 || _maxMile != 0)
                    {
                        // If min is set get all ads >= min
                        if (_minMile > 0 && _maxMile == 0)
                            bq.Add(NumericRangeQuery.NewIntRange("Mileage", _minMile, null, true, true), Occur.MUST);
                        // If max is set get all ads <=max
                        else if (_minMile == 0 && _maxMile > 0)
                            bq.Add(NumericRangeQuery.NewIntRange("Mileage", null, _maxMile, true, true), Occur.MUST);
                        // If max and min is set get all ads >= min && <= max
                        else if (_minMile == 0 && _maxMile == 0)
                            bq.Add(NumericRangeQuery.NewIntRange("Mileage", _minMile, _maxMile, true, true), Occur.MUST);
                    }

                    //model name
                    if (!string.IsNullOrEmpty(modelName))
                        bq.Add(new FuzzyQuery(new Term("Model", modelName)), Occur.MUST);

                    //max / min year
                    int _minYear = 0;
                    int _maxYear = 0;
                    int.TryParse(minYear, out _minYear);
                    int.TryParse(maxYear, out _maxYear);
                    if (_minYear != 0 || _maxYear != 0)
                    {
                        // If min is set get all ads >= min
                        if (_minYear > 0 && _maxYear == 0)
                            bq.Add(NumericRangeQuery.NewIntRange("Year", _minYear, null, true, true), Occur.MUST);
                        // If max is set get all ads <=max
                        else if (_minYear == 0 && _maxYear > 0)
                            bq.Add(NumericRangeQuery.NewIntRange("Year", null, _maxYear, true, true), Occur.MUST);
                        // If max and min is set get all ads >= min && <= max
                        else if (_minYear > 0 && _maxYear > 0)
                            bq.Add(NumericRangeQuery.NewIntRange("Year", _minYear, _maxYear, true, true), Occur.MUST);
                    }

                    //model body type
                    if (!string.IsNullOrEmpty(modelBodyType))
                        bq.Add(new TermQuery(new Term("Body Type", modelBodyType)), Occur.MUST);

                    //model drivetrain
                    if (!string.IsNullOrEmpty(modelDrivetrain))
                        bq.Add(new TermQuery(new Term("Drivetrain", modelDrivetrain)), Occur.MUST);

                    //model transmission
                    if (!string.IsNullOrEmpty(modelTransmission))
                        bq.Add(new TermQuery(new Term("Transmission", modelTransmission)), Occur.MUST);

                    //model condition
                    if (!string.IsNullOrEmpty(modelCondition))
                        bq.Add(new TermQuery(new Term("Condition", modelCondition)), Occur.MUST);

                    //model colour
                    if (!string.IsNullOrEmpty(modelColour))
                        bq.Add(new TermQuery(new Term("Colour", modelColour)), Occur.MUST);

                    //model engine size
                    if (!string.IsNullOrEmpty(modEngineSize))
                        bq.Add(new TermQuery(new Term("Engine Size", modEngineSize)), Occur.MUST);

                    //------JOBS---------
                    //model job type
                    if (!string.IsNullOrEmpty(modelJobType))
                        bq.Add(new TermQuery(new Term("Job Type", modelJobType)), Occur.MUST);

                    //model salary type
                    if (!string.IsNullOrEmpty(modelSalaryInfo))
                        bq.Add(new TermQuery(new Term("Salary Type", modelSalaryInfo)), Occur.MUST);

                    //-----REAL ESTATE----
                    //model min/max size
                    int _minSize = 0;
                    int _maxSize = 0;
                    int.TryParse(minSize, out _minSize);
                    int.TryParse(maxSize, out _maxSize);
                    if (_minSize != 0 || _maxSize != 0)
                    {
                        if (_minSize > 0 && _maxSize == 0)
                            bq.Add(NumericRangeQuery.NewIntRange("Size", _minSize, null, true, true), Occur.MUST);
                        else if (_minSize == 0 && _maxSize > 0)
                            bq.Add(NumericRangeQuery.NewIntRange("Size", null, _maxSize, true, true), Occur.MUST);
                        else if (_minSize > 0 && _maxSize > 0)
                            bq.Add(NumericRangeQuery.NewIntRange("Size", _minSize, _maxSize, true, true), Occur.MUST);
                    }

                    //model rental type
                    if(modelRentalType != null && modelRentalType != "")
                        bq.Add(new TermQuery(new Term("Rental Type", modelRentalType)), Occur.MUST);

                    //model bedrooms
                    if (modelBedrooms != null && modelBedrooms != "")
                        bq.Add(new TermQuery(new Term("Bedrooms", modelBedrooms)), Occur.MUST);

                    //model bathrooms
                    if (modelBathrooms != null && modelBathrooms != "")
                        bq.Add(new TermQuery(new Term("Bathrooms", modelBathrooms)), Occur.MUST);

                    //model furnished
                    if (modelFurnished != null && modelFurnished != "")
                        bq.Add(new TermQuery(new Term("Furnished", modelFurnished)), Occur.MUST);

                    //-----Pets--------
                    ///////////////////
                    if (modelSpecies != null && modelSpecies != "")
                        bq.Add(new TermQuery(new Term("Species", modelSpecies)), Occur.MUST);

                    bq.Add(new TermQuery(new Term("FeaturedAdStatus", "False")), Occur.MUST);

                    // set up lucene searcher
                    using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
                    {
                        var hits_limit = int.MaxValue;
                        var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                        var results = hits.ToPagedList(pageNumber.Value, RecordsPerPage.Records);
                        var returnlist = new StaticPagedList<ClassifiedAdList>(LuceneSearch._mapLuceneToDataList<ClassifiedAdList>(results, searcher), results.PageNumber, results.PageSize, results.TotalItemCount);
                        return returnlist;
                    }
                }
#if DEBUG
            }
#endif
        }

        public static IEnumerable<ClassifiedAdMinimal> GetCategoryTileFeaturedAds(int catId)
        {
            // Get 5 random featured ads from the given category
            // if < 5 available get random from all
            // LUCENE         
            // Setup query
            BooleanQuery bq = new BooleanQuery();
            var sortBy = new Sort();
            sortBy.SetSort(new SortField("TimeStampTicks", SortField.LONG, true), SortField.FIELD_SCORE);

            // Get only Open ads
            bq.Add(NumericRangeQuery.NewIntRange("Status", 0, 0, true, true), Occur.MUST);
            // Get Featured only
            bq.Add(new TermQuery(new Term("FeaturedAdStatus", "True")), Occur.MUST);

            bq.Add(new TermQuery(new Term("CategoryId", catId.ToString())), Occur.MUST);

            // Must have Pic
            bq.Add(new TermQuery(new Term("AdPhotos", "_NULL_")), Occur.MUST_NOT);

            // set up lucene searcher
            using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
            {
                var hits_limit = 1000;
                var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                Shuffle(hits);
                var results = LuceneSearch._mapLuceneToDataList<ClassifiedAdMinimal>(hits.Take(5), searcher);
                //Check if < 5
                if(results.Count() < 5)
                {
                    // get the remaining from regular list
                    bq = new BooleanQuery();
                    bq.Add(NumericRangeQuery.NewIntRange("Status", 0, 0, true, true), Occur.MUST);
                    bq.Add(new TermQuery(new Term("CategoryId", catId.ToString())), Occur.MUST);
                    bq.Add(new TermQuery(new Term("FeaturedAdStatus", "True")), Occur.MUST_NOT);
                    // Must have Pic
                    bq.Add(new TermQuery(new Term("AdPhotos", "_NULL_")), Occur.MUST_NOT);
                    hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                    Shuffle(hits);
                    var remainder = LuceneSearch._mapLuceneToDataList<ClassifiedAdMinimal>(hits.Take(5-results.Count()), searcher);
                    results = results.Concat(remainder);
                }
                return results;
            }            
        }

        public static IEnumerable<ClassifiedAdTitle> GetRecentClassifiedAdIndex(int catId)
        {
            // LUCENE
            // validation            
            // Setup query
            BooleanQuery bq = new BooleanQuery();
            var sortBy = new Sort();
            sortBy.SetSort(new SortField("TimeStampTicks", SortField.LONG, true), SortField.FIELD_SCORE);

            // Get only Open ads
            bq.Add(NumericRangeQuery.NewIntRange("Status", 0, 0, true, true), Occur.MUST);

            bq.Add(new TermQuery(new Term("CategoryId", catId.ToString())), Occur.MUST);
            // Must have Pic
            //bq.Add(new TermQuery(new Term("AdPhoto")), Occur.MUST);

            // set up lucene searcher
            using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
            {
                var hits_limit = 5;
                var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                var results = LuceneSearch._mapLuceneToDataList<ClassifiedAdTitle>(hits, searcher);

                return results;
            }            
        }

        public static IEnumerable<ClassifiedAdList> GetRecentClassifiedAdIndex()
        {
            // LUCENE
            // validation            
            // Setup query
            BooleanQuery bq = new BooleanQuery();
            var sortBy = new Sort();
            sortBy.SetSort(new SortField("TimeStampTicks", SortField.LONG, true), SortField.FIELD_SCORE);
            // Get only Open ads
            bq.Add(NumericRangeQuery.NewIntRange("Status", 0, 0, true, true), Occur.MUST);
            // Must have Pic
            //bq.Add(new TermQuery(new Term("AdPhoto")), Occur.MUST);

            // set up lucene searcher
            using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
            {
                var hits_limit = 5;
                var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                var results = LuceneSearch._mapLuceneToDataList<ClassifiedAdList>(hits, searcher);

                return results;
            }            
        }

        /// <summary>
        /// Get Featured ads based on category and subcategory
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="subCategoryId"></param>
        /// <param name="adType"></param>
        /// <returns></returns>
        public static IEnumerable<ClassifiedAdList> GetRefinedFeaturedAds(int? categoryId, int subCategoryId, int? countryId, int? regionId, string searchOnlyOption, string searchString, string adType = "ALL")
        {
            // LUCENE           
            // Setup query
            BooleanQuery bq = new BooleanQuery();
            var sortBy = new Sort();
            sortBy.SetSort(new SortField("TimeStampTicks", SortField.LONG, true), SortField.FIELD_SCORE);
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            {

                // Get search
                if (!string.IsNullOrEmpty(searchString))
                {
                    bq.Add(LuceneSearch.parseQuery(searchString, new MultiFieldQueryParser
                        (Lucene.Net.Util.Version.LUCENE_30, new[] {
                        "Title", "HtmlFreeDescription", "Country", "Region",
                        "Make", "Model", "Mileage", "Year", "Engine Size", "Condition", "Colour", "Fuel Type", "Transmission", "Drivetrain", "Body Type",
                        "Company Name", "Job Type",
                        "Species", "Age", "Pet's Name" }, analyzer))
                            , Occur.MUST);
                    sortBy = Sort.RELEVANCE;
                }

                // Get only Open ads
                bq.Add(NumericRangeQuery.NewIntRange("Status", 0, 0, true, true), Occur.MUST);
                // Get Featured only
                bq.Add(new TermQuery(new Term("FeaturedAdStatus", "True")), Occur.MUST);

                if (regionId.HasValue && regionId > 0)
                    bq.Add(NumericRangeQuery.NewIntRange("RegionId", regionId.Value, regionId.Value, true, true), Occur.MUST);
                else if (countryId.HasValue && countryId > 0)
                    bq.Add(NumericRangeQuery.NewIntRange("CountryId", countryId.Value, countryId.Value, true, true), Occur.MUST);

                if (searchOnlyOption.Equals("All Ads"))
                {
                    if (subCategoryId > 0)
                    {
                        if (adType.Equals("ALL"))
                        {
                            bq.Add(new TermQuery(new Term("SubCategoryId", subCategoryId.ToString())), Occur.MUST);
                        }
                        else
                        {
                            bq.Add(new TermQuery(new Term("SubCategoryId", subCategoryId.ToString())), Occur.MUST);
                            bq.Add(new TermQuery(new Term("AdType", adType)), Occur.MUST);
                        }
                    }
                    else if (categoryId.HasValue && categoryId > 0)
                    {
                        if (adType.Equals("ALL"))
                        {
                            bq.Add(new TermQuery(new Term("CategoryId", categoryId.Value.ToString())), Occur.MUST);
                        }
                        else
                        {
                            bq.Add(new TermQuery(new Term("CategoryId", categoryId.Value.ToString())), Occur.MUST);
                            bq.Add(new TermQuery(new Term("AdType", adType)), Occur.MUST);
                        }
                    }
                    else
                    {
                        if (!adType.Equals("ALL"))
                        {
                            bq.Add(new TermQuery(new Term("AdType", adType)), Occur.MUST);
                        }
                    }
                }
                else if (searchOnlyOption.Equals("Urgent Ads"))
                {
                    if (categoryId > 0)
                    {
                        if (adType.Equals("ALL"))
                        {
                            bq.Add(new TermQuery(new Term("CategoryId", categoryId.ToString())), Occur.MUST);
                            bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                        }
                        else
                        {
                            bq.Add(new TermQuery(new Term("CategoryId", categoryId.ToString())), Occur.MUST);
                            bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                            bq.Add(new TermQuery(new Term("AdType", adType)), Occur.MUST);
                        }
                    }
                    else if (subCategoryId > 0)
                    {
                        if (adType.Equals("ALL"))
                        {
                            bq.Add(new TermQuery(new Term("SubCategoryId", subCategoryId.ToString())), Occur.MUST);
                            bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                        }
                        else
                        {
                            bq.Add(new TermQuery(new Term("SubCategoryId", subCategoryId.ToString())), Occur.MUST);
                            bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                            bq.Add(new TermQuery(new Term("AdType", adType)), Occur.MUST);
                        }
                    }
                    else
                    {
                        if (!adType.Equals("ALL"))
                        {
                            bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                            bq.Add(new TermQuery(new Term("AdType", adType)), Occur.MUST);
                        }
                        else
                        {
                            bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                        }
                    }
                }

                // Must have Pic
                //bq.Add(new TermQuery(new Term("AdPhoto")), Occur.MUST);

                // set up lucene searcher
                using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
                {
                    var hits_limit = 1000;
                    var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                    var results = LuceneSearch._mapLuceneToDataListRandom<ClassifiedAdList>(hits, searcher);

                    return results;
                }
            }
        }

        public static IEnumerable<ClassifiedAdList> GetRefinedTopAds(int? categoryId, int subCategoryId, int? countryId, int? regionId, string searchOnlyOption, string searchString, string adType = "ALL")
        {
            // LUCENE           
            // Setup query
            BooleanQuery bq = new BooleanQuery();
            var sortBy = new Sort();
            sortBy.SetSort(new SortField("TimeStampTicks", SortField.LONG, true), SortField.FIELD_SCORE);
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            {

                // Get search
                if (!string.IsNullOrEmpty(searchString))
                {
                    bq.Add(LuceneSearch.parseQuery(searchString, new MultiFieldQueryParser
                        (Lucene.Net.Util.Version.LUCENE_30, new[] {
                        "Title", "HtmlFreeDescription", "Country", "Region",
                        "Make", "Model", "Mileage", "Year", "Engine Size", "Condition", "Colour", "Fuel Type", "Transmission", "Drivetrain", "Body Type",
                        "Company Name", "Job Type",
                        "Species", "Age", "Pet's Name" }, analyzer))
                            , Occur.MUST);
                    sortBy = Sort.RELEVANCE;
                }

                // Get only Open ads
                bq.Add(NumericRangeQuery.NewIntRange("Status", 0, 0, true, true), Occur.MUST);
                // Get Featured only
                bq.Add(new TermQuery(new Term("TopAdStatus", "True")), Occur.MUST);

                if (regionId.HasValue && regionId > 0)
                    bq.Add(NumericRangeQuery.NewIntRange("RegionId", regionId.Value, regionId.Value, true, true), Occur.MUST);
                else if (countryId.HasValue && countryId > 0)
                    bq.Add(NumericRangeQuery.NewIntRange("CountryId", countryId.Value, countryId.Value, true, true), Occur.MUST);

                if (searchOnlyOption.Equals("All Ads"))
                {
                    if (subCategoryId > 0)
                    {
                        if (adType.Equals("ALL"))
                        {
                            bq.Add(new TermQuery(new Term("SubCategoryId", subCategoryId.ToString())), Occur.MUST);
                        }
                        else
                        {
                            bq.Add(new TermQuery(new Term("SubCategoryId", subCategoryId.ToString())), Occur.MUST);
                            bq.Add(new TermQuery(new Term("AdType", adType)), Occur.MUST);
                        }
                    }
                    else if (categoryId.HasValue && categoryId > 0)
                    {
                        if (adType.Equals("ALL"))
                        {
                            bq.Add(new TermQuery(new Term("CategoryId", categoryId.Value.ToString())), Occur.MUST);
                        }
                        else
                        {
                            bq.Add(new TermQuery(new Term("CategoryId", categoryId.Value.ToString())), Occur.MUST);
                            bq.Add(new TermQuery(new Term("AdType", adType)), Occur.MUST);
                        }
                    }
                    else
                    {
                        if (!adType.Equals("ALL"))
                        {
                            bq.Add(new TermQuery(new Term("AdType", adType)), Occur.MUST);
                        }
                    }
                }
                else if (searchOnlyOption.Equals("Urgent Ads"))
                {
                    if (categoryId > 0)
                    {
                        if (adType.Equals("ALL"))
                        {
                            bq.Add(new TermQuery(new Term("CategoryId", categoryId.ToString())), Occur.MUST);
                            bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                        }
                        else
                        {
                            bq.Add(new TermQuery(new Term("CategoryId", categoryId.ToString())), Occur.MUST);
                            bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                            bq.Add(new TermQuery(new Term("AdType", adType)), Occur.MUST);
                        }
                    }
                    else if (subCategoryId > 0)
                    {
                        if (adType.Equals("ALL"))
                        {
                            bq.Add(new TermQuery(new Term("SubCategoryId", subCategoryId.ToString())), Occur.MUST);
                            bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                        }
                        else
                        {
                            bq.Add(new TermQuery(new Term("SubCategoryId", subCategoryId.ToString())), Occur.MUST);
                            bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                            bq.Add(new TermQuery(new Term("AdType", adType)), Occur.MUST);
                        }
                    }
                    else
                    {
                        if (!adType.Equals("ALL"))
                        {
                            bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                            bq.Add(new TermQuery(new Term("AdType", adType)), Occur.MUST);
                        }
                        else
                        {
                            bq.Add(new TermQuery(new Term("UrgentAdStatus", "True")), Occur.MUST);
                        }
                    }
                }

                // Must have Pic
                //bq.Add(new TermQuery(new Term("AdPhoto")), Occur.MUST);
                
                // set up lucene searcher
                using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
                {
                    var hits_limit = 1000;
                    var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                    var results = LuceneSearch._mapLuceneToDataListRandom<ClassifiedAdList>(hits, searcher);

                    return results;
                }
            }
        }

        public static IEnumerable<ClassifiedAdMinimal> GetRelatedClassifiedAd(string stringId, string price, int subcatId)
        {
            // LUCENE
            // validation            
            // Setup query
            BooleanQuery bq = new BooleanQuery();
            var sortBy = new Sort();
            sortBy.SetSort(new SortField("TimeStampTicks", SortField.LONG, true), SortField.FIELD_SCORE);
            bq.Add(NumericRangeQuery.NewIntRange("Status", 0, 0, true, true), Occur.MUST);
            bq.Add(new TermQuery(new Term("SubCategoryId", subcatId.ToString())), Occur.MUST);
            bq.Add(new TermQuery(new Term("StringId", stringId)), Occur.MUST_NOT);

            // set up lucene searcher
            using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
            {
                var hits_limit = 10;
                var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                var results = LuceneSearch._mapLuceneToDataList<ClassifiedAdMinimal>(hits, searcher);

                return results;
            }
        }

        public static IEnumerable<ClassifiedAdMinimal> GetFeaturedClassifiedAds()
        {
            // LUCENE
            // validation            
            // Setup query
            BooleanQuery bq = new BooleanQuery();
            var sortBy = new Sort();
            sortBy.SetSort(new SortField("TimeStampTicks", SortField.LONG, true), SortField.FIELD_SCORE);

            bq.Add(NumericRangeQuery.NewIntRange("Status", 0, 0, true, true), Occur.MUST);
            bq.Add(new TermQuery(new Term("FeaturedAdStatus", "True")), Occur.MUST);

            // set up lucene searcher
            using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
            {
                var hits_limit = 15;
                var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                var results = LuceneSearch._mapLuceneToDataList<ClassifiedAdMinimal>(hits, searcher);

                return results;
            }            
        }

        /// <summary>
        /// New classified Ad Search by Id from LUCENE
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static ClassifiedAdWithDetail GetClassifiedAdWithDetails(int Id)
        {
            // LUCENE
            BooleanQuery bq = new BooleanQuery();
            bq.Add(new TermQuery(new Term("Id", Id.ToString())), Occur.MUST);
            // set up lucene searcher
            using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
            {
                var hits_limit = 1;
                var hits = searcher.Search(bq, null, hits_limit).ScoreDocs;
                var results = LuceneSearch._mapLuceneToDataList<ClassifiedAdWithDetail>(hits, searcher);
                return results.FirstOrDefault();
            }            
        }

        public static ClassifiedAdWithDetail GetClassifiedAdWithDetailsFromDb(int Id)
        {
            using(var newthreadcontext = new ApplicationDbContext())
            {
                return newthreadcontext.ClassifiedDB.ProjectTo<ClassifiedAdWithDetail>().SingleOrDefault(x => x.Id == Id);
            }
        }        

        public static bool IsAdSuspended(int Id)
        {
            using(var newthreadcontext = new ApplicationDbContext())
            {
                return newthreadcontext.ClassifiedDB.Any(x => x.Id == Id && x.Status == -1);
            }
        }

        /// <summary>
        /// Get most popular ads
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ClassifiedAdList> GetRandomPickedClassifiedAds()
        {
            var randpicks = CacheHelper.GetFromCache<IEnumerable<ClassifiedAdList>>("tbhs-random-home-picks");
            if (randpicks == null)
            {
                // LUCENE         
                // Setup query
                BooleanQuery bq = new BooleanQuery();
                var sortBy = new Sort();
                sortBy.SetSort(new SortField("TimeStampTicks", SortField.LONG, true), SortField.FIELD_SCORE);
                // Get only Open ads
                bq.Add(NumericRangeQuery.NewIntRange("Status", 0, 0, true, true), Occur.MUST);
                // Get Featured only
                bq.Add(new TermQuery(new Term("FeaturedAdStatus", "False")), Occur.MUST);
                // Must have Pic
                bq.Add(new TermQuery(new Term("AdPhotos", "_NULL_")), Occur.MUST_NOT);
                // set up lucene searcher
                using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
                {
                    var hits_limit = 1000;
                    var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                    Shuffle(hits);
                    var results = LuceneSearch._mapLuceneToDataList<ClassifiedAdList>(hits.Take(5), searcher);
                    // save to cache for 5 mins
                    CacheHelper.SaveTocache("tbhs-random-home-picks", results, DateTime.Now.AddMinutes(5));
                    return results;
                }
            }
            return randpicks;
        }

        public static IPagedList<ClassifiedAdFavouriteList> GetUserFavouritedAds(IEnumerable<int> adIdList, int pageNumber = 1)
        {
            BooleanQuery bq = new BooleanQuery();
            var sortBy = new Sort();
            sortBy.SetSort(new SortField("TimeStampTicks", SortField.LONG, true), SortField.FIELD_SCORE);

            foreach(var adId in adIdList)
            {
                bq.Add(new TermQuery(new Term("Id", adId.ToString())), Occur.SHOULD);
            }

            // set up lucene searcher
            using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
            {
                var hits_limit = 1000;
                var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                var results = hits.ToPagedList(pageNumber, RecordsPerPage.Records);
                var returnlist = new StaticPagedList<ClassifiedAdFavouriteList>(LuceneSearch._mapLuceneToDataList<ClassifiedAdFavouriteList>(results, searcher), results.PageNumber, results.PageSize, results.TotalItemCount);
                return returnlist;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            if(n > 1)
            {
                for (int i = 0; i < n - 1; i++)
                {
                    var j = rng.Next(maxValue: n - 1, minValue: 0);
                    T value = list[j];
                    list[j] = list[i];
                    list[i] = value;
                }
            }          
        }
    }
}