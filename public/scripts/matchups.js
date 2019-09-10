function createTable (teams) {
    fetch('api/matchups')
    .then(function(response) {
        return response.json()
    }).then( teams => {
        let target = document.getElementById('target')
        let header = "<th></th>"
        let rows = ""
        for (let matchup of teams) {
            console.log(matchup)
            header += "<th>" + matchup.team + "</th>"
            rows += "<tr><th>" + matchup.team + "</th>"
            for (let result of matchup.matchups) {
                rows += "<td>" + result.result + "</td>"
            }
            rows += "</tr>"
        }
        let text = "<table>" + header + rows + "</table>"
        target.insertAdjacentHTML('afterbegin', text)
    });
}