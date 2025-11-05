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
        <button type="button" onclick="saveAllPlaceholder()">Save all</button>
        <span style="margin-left:10px">Rows: <span id="count">0</span></span>
        <span style="margin-left:10px">Status: <span id="status">idle</span></span>
    </div>

    <table>
        <thead>
            <tr>
                <th>Market</th>
                <th>Instrument</th>
                <th>Price (CZK)</th>
                <th>Best Bid</th>
                <th>Best Ask</th>
                <th>Last Update (UTC)</th>
            </tr>
        </thead>
        <tbody id="liveBody"></tbody>
    </table>
</form>
</body>
<script>
  console.log('[LiveData] script loaded');

  // ---------- helpers ----------
  const fmtNum = v => (typeof v === 'number' && !Number.isNaN(v))
    ? v.toLocaleString(undefined, { maximumFractionDigits: 2 }) : '';
  const fmtUtc = s => { try { return new Date(s).toISOString().replace('T',' ').replace('Z',''); } catch { return s ?? ''; } };
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
        <td style="text-align:right">${fmtNum(r.priceCzk)}</td>   <!-- CZK ONLY -->
        <td style="text-align:right">${fmtNum(r.bestBid)}</td>
        <td style="text-align:right">${fmtNum(r.bestAsk)}</td>
        <td>${fmtUtc(r.priceLastUpdateUtc)}</td>`;
      body.appendChild(tr);
    }
    const cnt = document.getElementById('count');
    if (cnt) cnt.textContent = rows.length.toString();
  }

  async function fetchLiveOnce() {
    try {
      setStatus('fetching…');
      const res = await fetch(baseUrl + '/api/exchangeRates/coindesk/btceur', { cache: 'no-store' });
      if (!res.ok) throw new Error('HTTP ' + res.status);
      const dto = await res.json();

      // Accept either PascalCase or camelCase from the server
      const row = {
        market:      pick(dto, 'market', 'Market'),
        instrument:  pick(dto, 'instrument', 'Instrument'),
        price:       toNum(pick(dto, 'price', 'Price')),
        bestBid:     toNum(pick(dto, 'bestBid', 'BestBid')),
        bestAsk:     toNum(pick(dto, 'bestAsk', 'BestAsk')),
        priceCzk:    toNum(pick(dto, 'priceCzk', 'PriceCzk')),
        priceLastUpdateUtc: pick(dto, 'priceLastUpdateUtc', 'PriceLastUpdateUtc')
      };

      if (!row.market && !row.instrument && Number.isNaN(row.price)) {
        setStatus('ignored empty payload at ' + new Date().toLocaleTimeString());
        console.warn('[LiveData] empty/invalid payload', dto);
        return;
      }

      rows.push(row);
      renderTable();
      setStatus('ok ' + new Date().toLocaleTimeString());
    } catch (e) {
      setStatus('error: ' + (e?.message ?? e));
      console.error('[LiveData] fetch error', e);
    }
  }

  // Auto-start on page load. Also wire body onload as a backup.
  function startPolling() {
    if (timerHandle) return;
    setStatus('booting…');
    baseUrl = window.location.origin;
    console.log('[LiveData] startPolling baseUrl=', baseUrl);
    fetchLiveOnce();                           // first fetch immediately
    timerHandle = setInterval(fetchLiveOnce, 5000);
  }

  function saveAllPlaceholder() {
    alert('Rows in memory: ' + rows.length + '\n(Next step wires this to POST /api/exchangeRates/snapshots/bulk)');
  }

  // Primary hook
  window.addEventListener('DOMContentLoaded', startPolling);
</script>
</html>
