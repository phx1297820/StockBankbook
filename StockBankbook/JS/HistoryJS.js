var data;
var mode;
function loadHistory() {
    axios({
        method: 'post',
        url: '/Home/History_GetHistory',
    })
        .then(function (response) {
            data = response.data;
            var rowsPerPage = 20;
            setPageContent(1, rowsPerPage);
            setPagination(data.length, rowsPerPage);
            mode = 'Close';
        })
        .catch(function (error) {
            console.log(error);
        });
}

function setPageContent(pageNumber, rowsPerPage) {
    var startIndex = (pageNumber - 1) * rowsPerPage;
    var endIndex = startIndex + rowsPerPage;
    var dataToDisplay = data.slice(startIndex, endIndex);
    var tableContent = document.getElementById('HistoryContent');

    for (var i = 0; i < dataToDisplay.length; i++) {
        //生成元素
        const logRow = document.createElement('tr');
        const IdCell = document.createElement('td');
        const DateCell = document.createElement('td');
        const ReasonCell = document.createElement('td');
        const PriceCell = document.createElement('td');
        const BalanceCell = document.createElement('td');
        //給元素值
        IdCell.textContent = dataToDisplay[i].Id;
        DateCell.textContent = new Date(parseInt(dataToDisplay[i].Date.substr(6))).toISOString().slice(0, 10);
        ReasonCell.textContent = dataToDisplay[i].Reason;
        PriceCell.textContent = dataToDisplay[i].Price;
        BalanceCell.textContent = dataToDisplay[i].Balance;
        //加入table
        logRow.appendChild(IdCell);
        logRow.appendChild(DateCell);
        logRow.appendChild(ReasonCell);
        logRow.appendChild(PriceCell);
        logRow.appendChild(BalanceCell);

        //設定deleteButtond和deleteCell
        const deleteButton = document.createElement('button');
        const deleteCell = document.createElement('td');
        //設定deleteCell一開始不可見
        deleteCell.setAttribute('hidden', '');
        //設定deleteButton內容
        deleteButton.textContent = 'Delete';
        deleteButton.classList.add('delete-button')
        deleteButton.addEventListener('click', function () {
            const row = this.parentNode.parentNode;
            if (confirm('確定要刪除第' + row.firstElementChild.textContent + '筆記錄嗎？')) {
                const result = row.firstElementChild.textContent;
                row.remove();
                axios({
                    method: 'post',
                    url: '/Home/Hostory_SaveHistory',
                    data: { result: result },
                })
                    .then(function (response) {
                        console.log(response);
                    })
                    .catch(function (error) {
                        console.log(error);
                    });
            }

        });
        //加入deleteButton
        deleteCell.appendChild(deleteButton);
        logRow.appendChild(deleteCell);
        tableContent.appendChild(logRow);
    }
}

function setPagination(dataLength, rowsPerPage) {
    var numPages = Math.ceil(dataLength / rowsPerPage);
    var paginationList = document.getElementById('pagination');
    var tableContent = document.getElementById('HistoryContent');

    for (var i = 1; i <= numPages; i++) {
        var link = document.createElement('a');
        link.href = '#';
        link.textContent = i;
        link.classList.add('page-link');
        link.addEventListener('click', function () {
            //刪除除了title(第零行)外的其他資料
            while (tableContent.firstChild) {
                tableContent.removeChild(tableContent.firstChild);
            }
            document.getElementById('using').classList.remove('active');
            document.getElementById('using').id = '';
            this.id = 'using';
            this.classList.add('active');
            setPageContent(parseInt(this.textContent), rowsPerPage);
            EditOrClose();
        });
        paginationList.appendChild(link);
    }
    paginationList.querySelector('a:first-child').id = 'using';
    paginationList.querySelector('a:first-child').classList.add('active');
}
function ChangeMode() {
    (mode == 'Edit') ? (mode = 'Close') : (mode = 'Edit');
    EditOrClose();
}

function EditOrClose() {
    const tableContent = document.getElementById('HistoryContent');
    const editButton = document.getElementById('edit');

    if (mode == 'Edit') {//未進入edit模式=>按下後"進入Edit模式"，文字顯示改為Close
        editButton.textContent = 'Close';
        for (let i = 0; i < tableContent.rows.length; i++) {
            //顯示editButton和deleteButton
            tableContent.rows[i].cells[5].hidden = false;
        }
    }
    else if (mode == 'Close') {//已進入edit模式=>按下後Close，文字顯示改為Edit
        editButton.textContent = 'Edit';
        for (let i = 0; i < tableContent.rows.length; i++) {
            //隱藏deleteButton
            tableContent.rows[i].cells[5].hidden = true;
        }
    }
}
