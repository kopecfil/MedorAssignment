<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="Saved.aspx.cs" Inherits="WebApp.Saved" %>
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
        <asp:Button ID="SaveChangesButton" runat="server" Text="Uložit změny" OnClick="SaveChangesButton_Click" />
        <asp:Button ID="DeleteSelectedButton" runat="server" Text="Smazat vybrané záznamy" OnClick="DeleteSelectedButton_Click" />
        &nbsp;&nbsp;Počet řádků: <asp:Label ID="RowCount" runat="server" Text="0" />
        &nbsp;&nbsp;<asp:Label ID="StatusLabel" runat="server" />
    </div>

    <asp:GridView ID="SavedGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="ExchangeRateEntryId">
        <Columns>
            <asp:TemplateField HeaderText="Vybrat">
                <ItemTemplate>
                    <asp:CheckBox ID="SelectRow" runat="server" />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" />
            </asp:TemplateField>

            <asp:BoundField DataField="ExchangeRateEntryId" HeaderText="ID" ReadOnly="True" />
            <asp:BoundField DataField="TimestampUtcDisplay" HeaderText="Čas (UTC)" ReadOnly="True" />
            <asp:BoundField DataField="PriceCzkDisplay" HeaderText="Cena (CZK)" ReadOnly="True">
                <ItemStyle CssClass="right" />
            </asp:BoundField>
            <asp:BoundField DataField="BestBidCzkDisplay" HeaderText="Nejlepší Bid (CZK)" ReadOnly="True">
                <ItemStyle CssClass="right" />
            </asp:BoundField>
            <asp:BoundField DataField="BestAskCzkDisplay" HeaderText="Nejlepší Ask (CZK)" ReadOnly="True">
                <ItemStyle CssClass="right" />
            </asp:BoundField>
            <asp:TemplateField HeaderText="Poznámka">
                <ItemTemplate>
                    <asp:TextBox ID="NoteBox" runat="server"
                                 Text='<%# Bind("UserNote") %>'
                                 Columns="40" />
                </ItemTemplate>
            </asp:TemplateField>

        </Columns>
    </asp:GridView>

</form>
</body>
</html>
