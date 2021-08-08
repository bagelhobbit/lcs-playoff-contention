function updateLeague(leagueName) {
    history.replaceState({}, leagueName + "Playoff Contention", "/" + leagueName.toLocaleLowerCase());

    if(leagueName != 'LCS') {
        document.getElementById('tab-update').style = "display: none";
        document.getElementById('cumulative').click();
    }
    else {
        document.getElementById('tab-update').style = "";
        let recordType = document.querySelector("#tab-update>ul>.is-active>a").id;
        document.getElementById(recordType).click();
    }

    document.querySelectorAll(".has-league").forEach((element) => {
        element.textContent = leagueName + element.textContent.substring(3);
    });
    document.getElementById('matchup-link').setAttribute('href', "/matchups/" + leagueName.toLocaleLowerCase())
}

window.addEventListener('DOMContentLoaded', async () => {
    let target = document.getElementById('target');
    let leagueElem = document.querySelector("[data-league]");
    let league = leagueElem.getAttribute("data-league");

    let headerResponse = await fetch('api/getSplitHeader/' + league);
    let headerText = await headerResponse.text();
    target.insertAdjacentHTML('afterbegin', headerText);

    let leagueLowercase = league.toLocaleLowerCase();
    let active = document.getElementById(leagueLowercase);
    active.parentElement.classList.add('is-active');

    if(leagueLowercase != "lcs") {
        document.getElementById('tab-update').style = "display: none";
    }

    let statusResponse = await fetch('api/getPlayoffStatuses/' + league + '/all/');
    let statusText = await statusResponse.text();
    leagueElem.style = "display:none;";
    target.insertAdjacentHTML('beforeend', statusText);

    let cumulative = document.getElementById('cumulative');
    let split = document.getElementById('split');

    cumulative.addEventListener('click', async () => {
        document.getElementById('records').remove();
        leagueElem.style = "";

        let league = document.querySelector("#tab-league>ul>.is-active>a").id
        statusResponse = await fetch('api/getPlayoffStatuses/' + league + '/all/');
        statusText = await statusResponse.text();
 
        cumulative.parentElement.classList.add('is-active');
        split.parentElement.classList.remove('is-active');
        leagueElem.style = "display:none;";
        target.insertAdjacentHTML('beforeend', statusText);
    });

    split.addEventListener('click', async () => {
        document.getElementById('records').remove();
        leagueElem.style = "";

        let league = document.querySelector("#tab-league>ul>.is-active>a").id
        statusResponse = await fetch('api/getPlayoffStatuses/' + league + '/split/');
        statusText = await statusResponse.text();

        split.parentElement.classList.add('is-active');
        cumulative.parentElement.classList.remove('is-active');
        leagueElem.style = "display:none;";
        target.insertAdjacentHTML('beforeend', statusText);
    });

    let lcs = document.getElementById('lcs');
    let lec = document.getElementById('lec');
    let lpl = document.getElementById('lpl');
    let lck = document.getElementById('lck');

    lcs.addEventListener('click', async () => {
        lcs.parentElement.classList.add('is-active');
        lec.parentElement.classList.remove('is-active');
        lpl.parentElement.classList.remove('is-active');
        lck.parentElement.classList.remove('is-active');

        updateLeague(lcs.textContent)
    });

    lec.addEventListener('click', async () => {
        lec.parentElement.classList.add('is-active');
        lcs.parentElement.classList.remove('is-active');
        lpl.parentElement.classList.remove('is-active');
        lck.parentElement.classList.remove('is-active');

        updateLeague(lec.textContent)
    });

    lpl.addEventListener('click', async () => {
        lpl.parentElement.classList.add('is-active');
        lcs.parentElement.classList.remove('is-active');
        lec.parentElement.classList.remove('is-active');
        lck.parentElement.classList.remove('is-active');

        updateLeague(lpl.textContent)
    });

    lck.addEventListener('click', async () => {
        lck.parentElement.classList.add('is-active');
        lcs.parentElement.classList.remove('is-active');
        lec.parentElement.classList.remove('is-active');
        lpl.parentElement.classList.remove('is-active');

        updateLeague(lck.textContent)
    });
});

