function loadBankbook() {
    axios({
        method: 'post',
        url: '/Home/Index_GetData',
    })
        .then(function (response) {
            var table1Content = document.getElementById("InfoContent");
            var table2Content = document.getElementById("StockContent");
            var data = response.data
            //列出投入損益
            const dataRow = document.createElement('tr');
            const NowBalanceCell = document.createElement('td');
            const NowStockValueCell = document.createElement('td');
            const NowAccountValueCell = document.createElement('td');
            const TotalCostCell = document.createElement('td');
            const ROICell = document.createElement('td');

            NowBalanceCell.textContent = data.NowBalance
            NowStockValueCell.textContent = data.NowStockValue
            NowAccountValueCell.textContent = data.NowAccountValue
            TotalCostCell.textContent = data.TotalCost
            ROICell.textContent = data.ROI

            dataRow.appendChild(NowBalanceCell);
            dataRow.appendChild(NowStockValueCell);
            dataRow.appendChild(NowAccountValueCell);
            dataRow.appendChild(TotalCostCell);
            dataRow.appendChild(ROICell);
            table1Content.appendChild(dataRow)

            //列出當前持有股票
            data.HeldStockList.forEach(function (HeldStockList) {

                const logRow = document.createElement('tr');
                const SymbolCell = document.createElement('td');
                const NameCell = document.createElement('td');
                const QuantityCell = document.createElement('td');
                const AverageCostCell = document.createElement('td');

                SymbolCell.textContent = HeldStockList.Symbol;
                NameCell.textContent = HeldStockList.Name;
                NameCell.contentEditable = "false";
                QuantityCell.textContent = HeldStockList.Quantity;
                AverageCostCell.textContent = Math.round(HeldStockList.AverageCost*100)/100;

                logRow.appendChild(SymbolCell);
                logRow.appendChild(NameCell);
                logRow.appendChild(QuantityCell);
                logRow.appendChild(AverageCostCell);
                table2Content.appendChild(logRow);
            });
        })
        .catch(function (error) {
            console.log(error);
        });

}

function EditOrFinish() {
    const tableContent = document.getElementById('StockContent');
    const editButton = document.getElementById('edit');
    const symbolColumn = 0;
    const nameColumn = 1;

    //檢查第一格能否被編輯
    if (tableContent.rows[0].cells[nameColumn].contentEditable == "false") {//第一格不能被編輯=>按下後"進入Edit模式"，文字顯示改為Finish
        editButton.textContent = 'Finish';
        for (let i = 0; i < tableContent.rows.length; i++) {
            const nameCell = tableContent.rows[i].cells[nameColumn];
            nameCell.contentEditable = true; // 設定可編輯
            nameCell.classList.add('edit-mode');
        }
    }
    else {//第一格不能被編輯=>按下後完成Edit，文字顯示改為Edit
        const tableData = [];
        editButton.textContent = 'Edit';
        for (let i = 0; i < tableContent.rows.length; i++) {
            const nameCell = tableContent.rows[i].cells[nameColumn];
            const symbolCell = tableContent.rows[i].cells[symbolColumn];
            nameCell.contentEditable = false; // 設定不可編輯
            nameCell.classList.remove('edit-mode');
            tableData.push(symbolCell.textContent);
            tableData.push(nameCell.textContent);
        }
        axios({
            method: 'post',
            url: '/Home/Index_SaveStockList',
            data: { tableData: tableData },
        })
            .then(function (response) {
                console.log(response);
            })
            .catch(function (error) {
                console.log(error);
            });
    }
}
