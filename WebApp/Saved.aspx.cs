using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using WebApp.Data;

namespace WebApp
{
    public partial class Saved : Page
    {
        private sealed class RowVm
        {
            public int ExchangeRateEntryId { get; set; }
            public string TimestampUtcDisplay { get; set; }
            public string PriceCzkDisplay { get; set; }
            public string BestBidCzkDisplay { get; set; }
            public string BestAskCzkDisplay { get; set; }
            public string UserNote { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    LoadAndBind();
                    StatusLabel.Text = "OK";
                }
                catch (Exception ex)
                {
                    // show the full error so we can see what's wrong
                    StatusLabel.Text = "Chyba: " + ex.Message;
#if DEBUG
                    // include stack in Debug to help while developing
                    StatusLabel.Text += "<br/><pre style='white-space:pre-wrap'>" + Server.HtmlEncode(ex.ToString()) + "</pre>";
#endif
                }
            }
        }

        private void LoadAndBind()
        {
            var cz = new CultureInfo("cs-CZ");
            using (var db = new ExchangeRatesDbContext())
            {
                // Oldest -> newest (you requested newest at the bottom)
                var items = db.ExchangeRateEntries
                              .OrderBy(x => x.TimestampUtc)
                              .ToList();

                var rows = new List<RowVm>(items.Count);
                foreach (var s in items)
                {
                    rows.Add(new RowVm
                    {
                        ExchangeRateEntryId = s.ExchangeRateEntryId,
                        TimestampUtcDisplay = s.TimestampUtc.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                        PriceCzkDisplay     = s.PriceCzk.ToString("C2", cz),
                        BestBidCzkDisplay   = s.BestBidCzk.ToString("C2", cz),
                        BestAskCzkDisplay   = s.BestAskCzk.ToString("C2", cz),
                        UserNote            = s.UserNote
                    });
                }

                SavedGrid.DataSource = rows;
                SavedGrid.DataBind();
                RowCount.Text = rows.Count.ToString();
            }
        }

        protected void SaveChangesButton_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "Ukládání změn zatím není implementováno.";
        }

        protected void DeleteSelectedButton_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "Mazání zatím není implementováno.";
        }
    }
}
