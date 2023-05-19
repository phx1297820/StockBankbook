function addStock() {
    var StockTable = document.getElementById('HeldStockList');
    const StockRow = document.createElement('tr');
    const StockSymbolCell = document.createElement('td');
    const StockNameCell = document.createElement('td');
    const StockQuantityCell = document.createElement('td');
    const StockTotalCostCell = document.createElement('td');
    const StockSymbolText = document.createElement('input');
    const StockNameText = document.createElement('input');
    const StockQuantityText = document.createElement('input');
    const StockTotalCostText = document.createElement('input');

    StockSymbolText.name = 'StockSymbol';
    StockNameText.name = 'StockName';
    StockQuantityText.name = 'StockQuantity';
    StockQuantityText.type='number';
    StockTotalCostText.name = 'StockTotalCost';
    StockTotalCostText.type = 'number';

    StockSymbolCell.appendChild(StockSymbolText);
    StockNameCell.appendChild(StockNameText);
    StockQuantityCell.appendChild(StockQuantityText);
    StockTotalCostCell.appendChild(StockTotalCostText);

    StockRow.appendChild(StockSymbolCell);
    StockRow.appendChild(StockNameCell);
    StockRow.appendChild(StockQuantityCell);
    StockRow.appendChild(StockTotalCostCell);

    StockTable.appendChild(StockRow);
}
