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
    <script>
        let baseUrl = '';
        let rows = []; // in-memory live buffer (not persisted)

        function formatIsoUtc(s) {
            try { return new Date(s).toISOString().replace('T',' ').replace('Z',''); }
            catch { return s; }
        }

        function renderTable() {
            const body = document.getElementById('liveBody');
            body.innerHTML = '';
            for (const r of rows) {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${r.market}</td>
                    <td>${r.instrument}</td>
                    <td style="text-align:right">${r.price.toLocaleString(undefined,{ maximumFractionDigits: 2 })}</td>
                    <td style="text-align:right">${r.bestBid?.toLocaleString(undefined,{ maximumFractionDigits: 2 }) ?? ''}</td>
                    <td style="text-align:right">${r.bestAsk?.toLocaleString(undefined,{ maximumFractionDigits: 2 }) ?? ''}</td>
                    <td>${formatIsoUtc(r.priceLastUpdateUtc)}</td>`;
                body.appendChild(tr);
            }
            document.getElementById('count').textContent = rows.length.toString();
        }

        async function fetchLiveOnce() {
            try {
                const res = await fetch(baseUrl + '/api/exchangeRates/coindesk/btceur', { cache: 'no-store' });
                if (!res.ok) throw new Error('HTTP ' + res.status);
                const dto = await res.json();
                rows.push(dto);
                renderTable();
            } catch (e) {
                console.log('fetch error', e);
            }
        }

        let timerHandle = null;
        function startPolling() {
            if (timerHandle) return;
            fetchLiveOnce();
            timerHandle = setInterval(fetchLiveOnce, 5000); // every 5s
        }

        function stopPolling() {
            if (timerHandle) {
                clearInterval(timerHandle);
                timerHandle = null;
            }
        }

        // Step 1: Save button will only alert how many rows we have (no DB yet)
        async function saveAllPlaceholder() {
            alert('Step 1 placeholder: rows in memory = ' + rows.length + '. We will wire this to POST /snapshots/bulk in the next step.');
        }

        window.addEventListener('load', () => {
            baseUrl = window.location.origin;
            startPolling();
        });
    </script>
</head>
<body>
<form id="form1" runat="server">
    <h2>Live data (BTC-EUR)</h2>
    <div class="controls">
        <button type="button" onclick="startPolling()">Start</button>
        <button type="button" onclick="stopPolling()">Stop</button>
        <button type="button" onclick="saveAllPlaceholder()">Save all</button>
        <span style="margin-left:10px">Rows: <span id="count">0</span></span>
    </div>
    <table>
        <thead>
            <tr>
                <th>Market</th>
                <th>Instrument</th>
                <th>Price (EUR)</th>
                <th>Best Bid</th>
                <th>Best Ask</th>
                <th>Last Update (UTC)</th>
            </tr>
        </thead>
        <tbody id="liveBody"></tbody>
    </table>
</form>
</body>
</html>
