async function createTable() {
    let response = await fetch('api/matchups');
    let teams = await response.json();
    
    let target = document.getElementById('target')
    let header = '<th></th>'
    let rows = ''
    let teamIndex = 0
    for (let matchup of teams) {
        header += '<th>' + matchup.team + '</th>'
        rows += '<tr><th>' + matchup.team + '</th>'
        let rowIndex = 0
        for (let result of matchup.matchups) {
            if (rowIndex === teamIndex) {
                rows += '<td></td>'
            }
            if (result.result === "Won") {
                className = 'has-text-success'
            }
            else if (result.result === "Tied") {
                className = 'has-text-info'
            }
            else {
                className = 'has-text-danger'
            }
            rows += '<td class="' + className +' title is-6">' + result.result + '</td>'
            rowIndex++
        }
        if (teamIndex === 9) {
            rows += '<td></td>'
        }
        rows += '</tr>'
        teamIndex++
    }
    let text = '<table>' + header + rows + '</table>'
    target.insertAdjacentHTML('afterbegin', text)
}