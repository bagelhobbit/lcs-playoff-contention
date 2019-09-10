window.addEventListener("DOMContentLoaded", () => {
    fetch('api/getSplitHeader')
    .then(function(response) {
        return response.text()
    }).then( text => {
        let target = document.getElementById('target')
        target.insertAdjacentHTML('afterbegin', text)
    });
    
    fetch('api/getPlayoffStatuses')
    .then(function(response) {
        return response.text()
    }).then( text => {
        let target = document.getElementById('target')
        target.insertAdjacentHTML('beforeend', text)
    })
});