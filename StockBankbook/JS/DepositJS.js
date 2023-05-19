function stockAddOption() {
    axios({
        method: 'get',
        url: '/Home/Deposit_GetStock',
    })
        .then(function (response) {
            // 將股票編號塞到select中
            var select = document.getElementById("Stock");
            response.data.forEach(function (stock) {
                const option = document.createElement("option");
                option.text = stock;
                option.value = stock;
                select.appendChild(option);
            });
        })
        .catch(function (error) {
            console.log(error);
        });

}

function summary1() {
    document.getElementById("Stock").setAttribute("hidden", "hidden");
    document.getElementById("Stock").removeAttribute("required");
    document.getElementById("Stock").value = null;
    document.getElementById("Detail").setAttribute("hidden", "hidden");
    document.getElementById("Detail").removeAttribute("required");
    document.getElementById("Detail").value = null;
}
function summary2() {
    document.getElementById("Stock").removeAttribute("hidden");
    document.getElementById("Stock").setAttribute("required", "required");
    document.getElementById("Detail").setAttribute("hidden", "hidden");
    document.getElementById("Detail").removeAttribute("required");
    document.getElementById("Detail").value = null;
}
function summary3() {
    document.getElementById("Stock").setAttribute("hidden", "hidden");
    document.getElementById("Stock").removeAttribute("required");
    document.getElementById("Stock").value = null;
    document.getElementById("Detail").removeAttribute("hidden");
    document.getElementById("Detail").setAttribute("required", "required");
}