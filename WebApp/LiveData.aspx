<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Live.aspx.cs" Inherits="WebApp.LiveData" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Live BTC/EUR</title>
    <style>
        table { border-collapse: collapse; }
        th, td { border: 1px solid #ccc; padding: 6px 10px; }
        .controls { margin: 10px 0; }
    </style>

</head>
<body>
<form id="form1" runat="server">
    <h2>Live data (BTC-EUR)</h2>
  <div class="controls">
    <button type="button" onclick="saveAll()">Uložit data</button>
    <button type="button" onclick="goToSaved()">Zobrazit uložená data</button>
    <span style="margin-left:10px">Řádků: <span id="count">0</span></span>
    <span style="margin-left:10px">Status: <span id="status">idle</span></span>
  </div>

  <table>
    <thead>
    <tr>
      <th>Trh</th>
      <th>Instrument</th>
      <th>Cena (CZK)</th>
      <th>Nejlepší Bid (CZK)</th>
      <th>Nejlepší Ask (CZK)</th>
      <th>Poslední aktualizace (UTC)</th>
    </tr>
    </thead>
    <tbody id="liveBody"></tbody>
  </table>
</form>
</body>
<script>
  console.log('[LiveData] script loaded');

  // ---------- localization ----------
  const LOCALE = 'cs-CZ';

  // todo naming omg
  const fmtCzk = v =>
    (typeof v === 'number' && !Number.isNaN(v))
      ? v.toLocaleString(LOCALE, { style: 'currency', currency: 'CZK', maximumFractionDigits: 2 })
      : '';

  const fmtUtc = s => {
    try {
      const d = new Date(s);
      // Czech date/time, seconds precision, no milliseconds
      const text = new Intl.DateTimeFormat(LOCALE, {
        year: 'numeric', month: '2-digit', day: '2-digit',
        hour: '2-digit', minute: '2-digit', second: '2-digit',
        hour12: false, timeZone: 'UTC'
      }).format(d);
      return text + ' UTC';
    } catch { return s ?? ''; }
  };

  const pick = (o, ...names) => { for (const n of names) { if (o && o[n] != null) return o[n]; } return null; };
  const toNum = v => (typeof v === 'number' ? v : (v == null ? NaN : Number(v)));

  // ---------- state ----------
  let baseUrl = '';
  let rows = [];
  let timerHandle = null;

  function setStatus(text) {
    const el = document.getElementById('status');
    if (el) el.textContent = text;
  }

  function renderTable() {
    const body = document.getElementById('liveBody');
    if (!body) return;

    body.innerHTML = '';
    for (const r of rows) {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${r.market ?? ''}</td>
        <td>${r.instrument ?? ''}</td>
        <td style="text-align:right">${fmtCzk(r.priceCzk)}</td>
        <td style="text-align:right">${fmtCzk(r.bestBidCzk)}</td>
        <td style="text-align:right">${fmtCzk(r.bestAskCzk)}</td>
        <td>${fmtUtc(r.priceLastUpdateUtc)}</td>`;
      body.appendChild(tr);
    }
    const cnt = document.getElementById('count');
    if (cnt) cnt.textContent = rows.length.toString();
  }

  async function fetchLiveOnce() {
    try {
      setStatus('načítám…');
      const res = await fetch(baseUrl + '/api/exchangeRates/coindesk/btceur', { cache: 'no-store' });
      if (!res.ok) throw new Error('HTTP ' + res.status);
      const dto = await res.json();

      // Accept either camelCase or PascalCase from the server
      const priceEur = toNum(pick(dto, 'price', 'Price'));
      const priceCzk = toNum(pick(dto, 'priceCzk', 'PriceCzk'));
      const bestBidEur = toNum(pick(dto, 'bestBid', 'BestBid'));
      const bestAskEur = toNum(pick(dto, 'bestAsk', 'BestAsk'));
      // Prefer CZK from API; if missing, derive CZK using EUR->CZK ratio from price
      const ratio = (Number.isFinite(priceEur) && priceEur !== 0 && Number.isFinite(priceCzk))
        ? (priceCzk / priceEur) : NaN;

      const row = {
        market:      pick(dto, 'market', 'Market'),
        instrument:  pick(dto, 'instrument', 'Instrument'),
        priceCzk:    Number.isFinite(priceCzk) ? priceCzk : NaN,
        bestBidCzk:  toNum(pick(dto, 'bestBidCzk', 'BestBidCzk')),
        bestAskCzk:  toNum(pick(dto, 'bestAskCzk', 'BestAskCzk')),
        priceLastUpdateUtc: pick(dto, 'priceLastUpdateUtc', 'PriceLastUpdateUtc')
      };

      // Derive CZK for bid/ask if API doesn't provide them yet
      if (!Number.isFinite(row.bestBidCzk) && Number.isFinite(bestBidEur) && Number.isFinite(ratio)) {
        row.bestBidCzk = bestBidEur * ratio;
      }
      if (!Number.isFinite(row.bestAskCzk) && Number.isFinite(bestAskEur) && Number.isFinite(ratio)) {
        row.bestAskCzk = bestAskEur * ratio;
      }

      // Guard against empty/invalid payload
      const hasAny = row.market || row.instrument || Number.isFinite(row.priceCzk);
      if (!hasAny) {
        setStatus('ignorováno (prázdná odpověď) ' + new Date().toLocaleTimeString(LOCALE));
        console.warn('[LiveData] empty/invalid payload', dto);
        return;
      }

      rows.push(row);
      renderTable();
      setStatus('OK ' + new Date().toLocaleTimeString(LOCALE));
    } catch (e) {
      setStatus('chyba: ' + (e?.message ?? e));
      console.error('[LiveData] fetch error', e);
    }
  }

  function startPolling() {
    if (timerHandle) return;
    baseUrl = window.location.origin;
    fetchLiveOnce();
    // todo note - update interval to be configurable
    timerHandle = setInterval(fetchLiveOnce, 5000);
  }

  async function saveAll() {
    try {
      const payload = {
        items: rows.map(r => ({
          timestampUtc: r.priceLastUpdateUtc, // ISO string is fine; Web API will parse to DateTime (UTC)
          priceCzk: r.priceCzk,
          bestBidCzk: r.bestBidCzk,
          bestAskCzk: r.bestAskCzk,
          market: r.market || "coinbase",
          instrument: r.instrument || "BTC-EUR"
        }))
      };

      const res = await fetch(baseUrl + '/api/exchangeRates/snapshots/bulk', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (!res.ok) {
        const txt = await res.text();
        setStatus('chyba ukládání: ' + txt);
        return;
      }

      const j = await res.json();
      setStatus('uloženo: ' + j.inserted);
      // per your requirement: keep page running, do NOT clear rows or stop polling
    } catch (e) {
      setStatus('chyba ukládání: ' + (e?.message ?? e));
      console.error(e);
    }
  }
  
  function goToSaved() {
    window.location.href = window.location.origin + "/SavedData";
  }

  window.addEventListener('DOMContentLoaded', startPolling);
  
</script>

</html>
