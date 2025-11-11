using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using WebApp.Data;
using static WebApp.Basic.Utils;

namespace WebApp
{
    public partial class SavedData : Page
    {
        private string SortExpression
        {
            get => (string)(ViewState["SortExpression"] ?? "TimestampUtc");
            set => ViewState["SortExpression"] = value;
        }

        private bool SortAscending
        {
            get => (bool?)ViewState["SortAscending"] ?? true;// oldest -> newest by default
            set => ViewState["SortAscending"] = value;
        }

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
            if(!IsPostBack)
            {
                try
                {
                    LoadAndBind();
                    StatusLabel.Text = "OK";
                }
                catch(Exception ex)
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
            var cz = new System.Globalization.CultureInfo("cs-CZ");

            DateTime? fromUtc = ParseUtcOrNull(FromUtcText.Text);
            DateTime? toUtc = ParseUtcOrNull(ToUtcText.Text);
            decimal? priceMin = ParseDecOrNull(PriceMinText.Text);
            decimal? priceMax = ParseDecOrNull(PriceMaxText.Text);
            decimal? bidMin = ParseDecOrNull(BidMinText.Text);
            decimal? bidMax = ParseDecOrNull(BidMaxText.Text);
            decimal? askMin = ParseDecOrNull(AskMinText.Text);
            decimal? askMax = ParseDecOrNull(AskMaxText.Text);
            string noteLike = (NoteContainsText.Text ?? "").Trim();

            using(var db = new WebApp.Data.ExchangeRatesDbContext())
            {
                var q = db.ExchangeRateEntries.AsQueryable();

                if(fromUtc.HasValue) q = q.Where(x => x.TimestampUtc >= fromUtc.Value);
                if(toUtc.HasValue) q = q.Where(x => x.TimestampUtc <= toUtc.Value);

                if(priceMin.HasValue) q = q.Where(x => x.PriceCzk >= priceMin.Value);
                if(priceMax.HasValue) q = q.Where(x => x.PriceCzk <= priceMax.Value);

                if(bidMin.HasValue) q = q.Where(x => x.BestBidCzk >= bidMin.Value);
                if(bidMax.HasValue) q = q.Where(x => x.BestBidCzk <= bidMax.Value);

                if(askMin.HasValue) q = q.Where(x => x.BestAskCzk >= askMin.Value);
                if(askMax.HasValue) q = q.Where(x => x.BestAskCzk <= askMax.Value);

                if(!string.IsNullOrEmpty(noteLike)) q = q.Where(x => x.UserNote.Contains(noteLike));

                // sorting (you already have SortExpression/SortAscending)
                switch(SortExpression)
                {
                    case "ExchangeRateEntryId":
                        q = (SortAscending ? q.OrderBy(x => x.ExchangeRateEntryId) : q.OrderByDescending(x => x.ExchangeRateEntryId));
                        break;
                    case "TimestampUtc":
                        q = (SortAscending ? q.OrderBy(x => x.TimestampUtc) : q.OrderByDescending(x => x.TimestampUtc));
                        break;
                    case "PriceCzk":
                        q = (SortAscending ? q.OrderBy(x => x.PriceCzk) : q.OrderByDescending(x => x.PriceCzk));
                        break;
                    case "BestBidCzk":
                        q = (SortAscending ? q.OrderBy(x => x.BestBidCzk) : q.OrderByDescending(x => x.BestBidCzk));
                        break;
                    case "BestAskCzk":
                        q = (SortAscending ? q.OrderBy(x => x.BestAskCzk) : q.OrderByDescending(x => x.BestAskCzk));
                        break;
                    case "UserNote":
                        q = (SortAscending ? q.OrderBy(x => x.UserNote) : q.OrderByDescending(x => x.UserNote));
                        break;
                    default:
                        q = (SortAscending ? q.OrderBy(x => x.TimestampUtc) : q.OrderByDescending(x => x.TimestampUtc));
                        break;
                }

                var items = q.ToList();

                var rows = new List<RowVm>(items.Count);
                foreach(var s in items)
                {
                    rows.Add(new RowVm
                    {
                        ExchangeRateEntryId = s.ExchangeRateEntryId,
                        TimestampUtcDisplay = s.TimestampUtc.ToString("dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                        PriceCzkDisplay = s.PriceCzk.ToString("C2", cz),
                        BestBidCzkDisplay = s.BestBidCzk.ToString("C2", cz),
                        BestAskCzkDisplay = s.BestAskCzk.ToString("C2", cz),
                        UserNote = s.UserNote
                    });
                }

                SavedGrid.DataSource = rows;
                SavedGrid.DataBind();
                RowCount.Text = rows.Count.ToString();
                StatusLabel.Text = $"OK (filtr aktivní; řazení {SortExpression} {(SortAscending ? "↑" : "↓")})";
            }
        }

        protected void SaveChangesButton_Click(object sender, EventArgs e)
        {
            try
            {
                var idsAndNotes = new List<(int id, string note)>();
                foreach(System.Web.UI.WebControls.GridViewRow row in SavedGrid.Rows)
                {
                    var id = (int)SavedGrid.DataKeys[row.RowIndex].Value;
                    var noteBox = (System.Web.UI.WebControls.TextBox)row.FindControl("NoteBox");
                    var note = (noteBox?.Text ?? string.Empty).Trim();

                    if(string.IsNullOrEmpty(note))
                    {
                        StatusLabel.Text = $"Poznámka nesmí být prázdná (řádek ID {id}).";
                        return;// fail fast; nothing saved
                    }
                    idsAndNotes.Add((id, note));
                }

                var nowUtc = DateTime.UtcNow;
                using(var db = new WebApp.Data.ExchangeRatesDbContext())
                {
                    var ids = idsAndNotes.Select(x => x.id).ToArray();
                    var map = idsAndNotes.ToDictionary(x => x.id, x => x.note);

                    var entities = db.ExchangeRateEntries.Where(x => ids.Contains(x.ExchangeRateEntryId)).ToList();
                    foreach(var entity in entities)
                    {
                        entity.UserNote = map[entity.ExchangeRateEntryId];
                        entity.UpdatedAtUtc = nowUtc;
                    }
                    db.SaveChanges();
                }

                StatusLabel.Text = $"Změny uloženy ({idsAndNotes.Count}).";
            }
            catch(Exception ex)
            {
                StatusLabel.Text = "Chyba při ukládání: " + ex.Message;
            }
        }

        protected void DeleteSelectedButton_Click(object sender, EventArgs e)
        {
            try
            {
                // ensure user confirmed
                // this seems like a weird way to do confirmation, but im not an expert on FE
                var confirm = Request.Form["confirm_value"];
                if(!string.Equals(confirm, "Ano", StringComparison.OrdinalIgnoreCase))
                {
                    StatusLabel.Text = "Mazání zrušeno.";
                    return;
                }

                // collect selected IDs
                var ids = new List<int>();
                foreach(System.Web.UI.WebControls.GridViewRow row in SavedGrid.Rows)
                {
                    var cb = (System.Web.UI.WebControls.CheckBox)row.FindControl("SelectRow");
                    if(cb != null && cb.Checked)
                    {
                        var id = (int)SavedGrid.DataKeys[row.RowIndex].Value;
                        ids.Add(id);
                    }
                }

                if(ids.Count == 0)
                {
                    StatusLabel.Text = "Není vybrán žádný záznam.";
                    return;
                }

                // delete in one go
                using(var db = new WebApp.Data.ExchangeRatesDbContext())
                {
                    var toRemove = db.ExchangeRateEntries.Where(x => ids.Contains(x.ExchangeRateEntryId)).ToList();
                    if(toRemove.Count == 0)
                    {
                        StatusLabel.Text = "Vybrané záznamy nebyly nalezeny (možná již smazány).";
                    }
                    else
                    {
                        db.ExchangeRateEntries.RemoveRange(toRemove);
                        db.SaveChanges();
                        StatusLabel.Text = $"Smazáno {toRemove.Count} záznamů.";
                    }
                }

                // refresh grid + count
                LoadAndBind();
                RowCount.Text = SavedGrid.Rows.Count.ToString();
            }
            catch(Exception ex)
            {
                StatusLabel.Text = "Chyba při mazání: " + ex.Message;
            }
        }
        
        protected void SavedGrid_Sorting(object sender, System.Web.UI.WebControls.GridViewSortEventArgs e)
        {
            if (string.Equals(SortExpression, e.SortExpression, StringComparison.OrdinalIgnoreCase))
                SortAscending = !SortAscending;
            else
            {
                SortExpression = e.SortExpression;
                SortAscending = true;
            }
            LoadAndBind();
        }

        protected void FilterButton_Click(object sender, EventArgs e)
        {
            LoadAndBind();
        }

        protected void ClearFiltersButton_Click(object sender, EventArgs e)
        {
            FromUtcText.Text = ToUtcText.Text = "";
            PriceMinText.Text = PriceMaxText.Text = BidMinText.Text = BidMaxText.Text = AskMinText.Text = AskMaxText.Text = "";
            NoteContainsText.Text = "";
            LoadAndBind();
        }
    }
}
