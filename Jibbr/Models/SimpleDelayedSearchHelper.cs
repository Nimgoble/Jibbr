using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using Caliburn.Micro;

namespace Jibbr.Models
{
    public class SimpleDelayedSearchHelper<T>
    {
        #region Private Members
        private Timer searchTimer;

        //JH test:
        private ICollection<T> sourceContainer = null;
        private ICollection<T> searchResults = null;
        private System.Linq.Expressions.Expression<Func<T, bool>> searchExpression;
        private bool defaultToSourceCollectionOnEmpty = false;
        #endregion

        #region Constructors
        public SimpleDelayedSearchHelper
        (
            Double searchInterval,
            ref ICollection<T> sourceContainer,
            ref ICollection<T> searchResults,
            System.Linq.Expressions.Expression<Func<T, bool>> searchExpression,
            bool defaultToSourceCollectionOnEmpty = false
        )
        {
            this.sourceContainer = sourceContainer;
            this.searchResults = searchResults;
            this.searchExpression = searchExpression;
            this.defaultToSourceCollectionOnEmpty = defaultToSourceCollectionOnEmpty;

            searchTimer = new Timer() { Interval = searchInterval, AutoReset = false };
            searchTimer.Elapsed += new ElapsedEventHandler(DoSearch);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Refresh the search results.  RIGHT NOW.
        /// </summary>
        public void ForceRefresh()
        {
            searchTimer.Stop();//Is this right?
            //Lock the searchMutex, so we only process one search at a time.
            lock (searchMutex)
            {
                LocklessDoSearch();
            }
        }
        /// <summary>
        /// Mutex that makes sure DoSearch doesn't run more than once at a time.
        /// </summary>
        private object searchMutex = new object();
        /// <summary>
        /// Search for items in the background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoSearch(object sender, ElapsedEventArgs e)
        {
            //Lock the searchMutex, so we only process one search at a time.
            lock (searchMutex)
            {
                LocklessDoSearch();
            }
        }
        /// <summary>
        /// Performs a search without locking
        /// </summary>
        private void LocklessDoSearch()
        {
            String tempSearchText = String.Copy(SearchText);
            IEnumerable<T> rtn = sourceContainer.Where(searchExpression.Compile());
            Execute.BeginOnUIThread
            (
                () =>
                {
                    searchResults.Clear();
                    IEnumerable<T> targetCollection = (rtn.Count() == 0) ? ((defaultToSourceCollectionOnEmpty) ? sourceContainer : rtn) : rtn;
                    foreach (T item in targetCollection)
                        searchResults.Add(item);
                }
            );
        }
        #endregion

        #region Properties
        /// <summary>
        /// The action to call when it's time to search
        /// </summary>
        private Func<String, IEnumerable<T>> searchAction = null;
        public Func<String, IEnumerable<T>> SearchAction
        {
            get { return searchAction; }
            set { searchAction = value; }
        }
        /// <summary>
        /// The action to call when our search is done.
        /// </summary>
        private System.Action<IEnumerable<T>> searchDoneAction = null;
        public System.Action<IEnumerable<T>> SearchDoneAction
        {
            get { return searchDoneAction; }
            set { searchDoneAction = value; }
        }
        /// <summary>
        /// Our search text
        /// </summary>
        private object searchStringMutex = new object();
        private String searchText = String.Empty;
        public String SearchText
        {
            get
            {
                String rtn = String.Empty;
                lock (searchStringMutex)
                {
                    rtn = String.Copy(searchText);
                }
                return rtn;
            }
            set
            {
                lock (searchStringMutex)
                {
                    if (value != searchText)
                        searchText = value;
                }

                searchTimer.Stop();
                searchTimer.Start();
            }
        }
        #endregion
    }
}
