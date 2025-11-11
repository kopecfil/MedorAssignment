<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="~/SavedData.aspx.cs" Inherits="WebApp.SavedData" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Uložená data</title>
    <style>
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ccc; padding: 6px 10px; }
        .right { text-align: right; }
        .toolbar { margin: 10px 0; }
        .muted { color:#666; font-size: 0.9em; }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <h2>Uložená data</h2>

    <div class="toolbar">
        <span>Od (UTC):</span>
        <asp:TextBox ID="FromUtcText" runat="server" Width="140" placeholder="dd.MM.yyyy HH:mm:ss" />
        &nbsp;<span>Do (UTC):</span>
        <asp:TextBox ID="ToUtcText" runat="server" Width="140" placeholder="dd.MM.yyyy HH:mm:ss" />
        <br />
        <span>Cena CZK ≥</span>
        <asp:TextBox ID="PriceMinText" runat="server" Width="90" />
        &nbsp;<span>≤</span>
        <asp:TextBox ID="PriceMaxText" runat="server" Width="90" />
        &nbsp;&nbsp;<span>Bid CZK ≥</span>
        <asp:TextBox ID="BidMinText" runat="server" Width="90" />
        &nbsp;<span>≤</span>
        <asp:TextBox ID="BidMaxText" runat="server" Width="90" />
        &nbsp;&nbsp;<span>Ask CZK ≥</span>
        <asp:TextBox ID="AskMinText" runat="server" Width="90" />
        &nbsp;<span>≤</span>
        <asp:TextBox ID="AskMaxText" runat="server" Width="90" />
        <br />
        &nbsp;&nbsp;<span>Poznámka obsahuje:</span>
        <asp:TextBox ID="NoteContainsText" runat="server" Width="180" />
        &nbsp;
        <asp:Button ID="FilterButton" runat="server" Text="Filtrovat" OnClick="FilterButton_Click" />
        <asp:Button ID="ClearFiltersButton" runat="server" Text="Zrušit filtr" OnClick="ClearFiltersButton_Click" />
    </div>

    
    <div class="toolbar">
        <asp:Button ID="SaveChangesButton" runat="server" Text="Uložit změny" OnClick="SaveChangesButton_Click" />
        <asp:Button ID="DeleteSelectedButton" runat="server"
                    Text="Smazat vybrané záznamy"
                    OnClientClick="return confirmDelete();"
                    OnClick="DeleteSelectedButton_Click" />
        <input type="hidden" id="confirm_value" name="confirm_value" />
        &nbsp;&nbsp;Počet řádků: <asp:Label ID="RowCount" runat="server" Text="0" />
        &nbsp;&nbsp;<asp:Label ID="StatusLabel" runat="server" />
    </div>

    <asp:GridView ID="SavedGrid" runat="server"
                  AutoGenerateColumns="False"
                  DataKeyNames="ExchangeRateEntryId"
                  AllowSorting="True"
                  OnSorting="SavedGrid_Sorting">
        <Columns>
            <asp:TemplateField HeaderText="">
                <ItemTemplate>
                    <asp:CheckBox ID="SelectRow" runat="server" />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>

            <asp:BoundField DataField="ExchangeRateEntryId" HeaderText="ID" ReadOnly="True" SortExpression="ExchangeRateEntryId" />
            <asp:BoundField DataField="TimestampUtcDisplay" HeaderText="Čas (UTC)" ReadOnly="True" SortExpression="TimestampUtc" />
            <asp:BoundField DataField="PriceCzkDisplay" HeaderText="Cena (CZK)" ReadOnly="True" SortExpression="PriceCzk">
                <ItemStyle CssClass="right" />
            </asp:BoundField>
            <asp:BoundField DataField="BestBidCzkDisplay" HeaderText="Nejlepší Bid (CZK)" ReadOnly="True" SortExpression="BestBidCzk">
                <ItemStyle CssClass="right" />
            </asp:BoundField>
            <asp:BoundField DataField="BestAskCzkDisplay" HeaderText="Nejlepší Ask (CZK)" ReadOnly="True" SortExpression="BestAskCzk">
                <ItemStyle CssClass="right" />
            </asp:BoundField>

            <asp:TemplateField HeaderText="Poznámka" SortExpression="UserNote">
                <ItemTemplate>
                    <asp:TextBox ID="NoteBox" runat="server" Text='<%# Bind("UserNote") %>' Columns="40" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <script type="text/javascript">
      function confirmDelete() {
        var confirmed = confirm("Opravdu chcete smazat vybrané záznamy?");
        if (confirmed) document.getElementById("confirm_value").value = "Ano";
        return confirmed;
      }
      function toggleSelectAll(source) {
        var boxes = document.querySelectorAll('input[id$=SelectRow]');
        for (var i = 0; i < boxes.length; i++) boxes[i].checked = source.checked;
      }
    </script>

</form>
</body>
</html>
