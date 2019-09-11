async function createTable() {
    let response = await fetch('api/matchups');
    let teams = await response.json();
    
    let target = document.getElementById('target')
    let header = '<th></th>'
    let rows = ''
    for (let matchup of teams) {
        console.log(matchup)
        header += '<th>' + matchup.team + '</th>'
        rows += '<tr><th>' + matchup.team + '</th>'
        for (let result of matchup.matchups) {
            rows += '<td>' + result.result + '</td>'
        }
        rows += '</tr>'
    }
    let text = '<table>' + header + rows + '</table>'
    target.insertAdjacentHTML('afterbegin', text)
}