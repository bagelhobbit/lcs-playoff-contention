async function createTable() {
    let response = await fetch('api/matchups');
    let teams = await response.json();

    let target = document.getElementById('target')
    let header = '<th></th>'
    let rows = ''
    let teamIndex = 0
    for (let matchup of teams) {
        let teamMatchupLink = '<a href="/matchups/' + matchup.teamCode + '">' + matchup.team + '</a>'
        header += '<th>' + teamMatchupLink + '</th>'
        rows += '<tr><th>' + teamMatchupLink + '</th>'
        let rowIndex = 0
        for (let i = 0; i < teams.length - 1; i++) {
            if (rowIndex === teamIndex) {
                rows += '<td></td>'
            }
            if (matchup.matchups[i] !== undefined) {
                if (matchup.matchups[i].result === "Won") {
                    className = 'has-text-success'
                }
                else if (matchup.matchups[i].result === "Tied") {
                    className = 'has-text-info'
                }
                else {
                    className = 'has-text-danger'
                }

                rows += '<td class="' + className + ' title is-6">' + matchup.matchups[i].result + '</td>'
            }
            else {
                rows += '<td></td>'
            }

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