using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Extensions;

namespace Trinbago_MVC5.Controllers
{
    public class PhotoManager : IDisposable
    {
        private IndexSearcher _searcher;
        
        public IndexSearcher Searcher { get { return _searcher ?? new IndexSearcher(LuceneSearch._getDir, false); } set { _searcher = value;  } }

        public ClassifiedAdListPhoto GetAdDetailImageBytes(int Id, string FileName)
        {
            // LUCENE
            BooleanQuery bq = new BooleanQuery
            {
                { new TermQuery(new Term("AdPhotoLocatorId", string.Format("{0}-{1}", Id, FileName))), Occur.MUST }
            };
            // set up lucene searcher

            var hits_limit = 1;
            var hits = Searcher.Search(bq, hits_limit).ScoreDocs;
            var results = LuceneSearch._mapLuceneToDataList<ClassifiedAdListPhoto>(hits, Searcher);
            return results.FirstOrDefault();                        
        }

        public void Dispose()
        {
            _searcher.Dispose();
            _searcher = null;
        }
    }
}