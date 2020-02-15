async function createTable() {
    let response = await fetch('api/matchups/matchups');
    let teams = await response.json();

    let target = document.getElementById('target')
    let header = '<th></th>'
    let rows = ''
    let teamIndex = 0
    for (let matchup of teams) {
        console.log(matchup.matchups)
        let teamMatchupLink = '<a href="/matchups/' + matchup.teamCode + '">' + matchup.team + '</a>'
        header += '<th>' + teamMatchupLink + '</th>'
        rows += '<tr><th>' + teamMatchupLink + '</th>'
        let rowIndex = 0
        for (let i = 0; i < teams.length; i++) {
            if (matchup.matchups[i].result === "Won") {
                className = 'has-text-success'
            }
            else if (matchup.matchups[i].result === "Tied") {
                className = 'has-text-info'
            }
            else if (matchup.matchups[i].result === "Lost") {
                className = 'has-text-danger'
            }
            else {
                className = ""
            }

            if (rowIndex === teamIndex) {
                rows += '<td>X</td>'
            }
            else {
                rows += '<td class="' + className + ' title is-6">' + matchup.matchups[i].result + '</td>'
            }

            rowIndex++
        }

        rows += '</tr>'
        teamIndex++
    }
    let text = '<table>' + header + rows + '</table>'
    target.insertAdjacentHTML('afterbegin', text)
}