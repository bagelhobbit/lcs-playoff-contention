function updateLeague(leagueName) {
    let recordType = document.querySelector("#tab-update>ul>.is-active>a").id;
    document.getElementById(recordType).click();
    document.querySelectorAll(".has-league").forEach((element) => {
        element.textContent = leagueName + element.textContent.substring(3);
    });
    document.getElementById('matchup-link').setAttribute('href', "/matchups/" + leagueName.toLocaleLowerCase())

    if(leagueName != 'LCS'){
        document.getElementById('tab-update').style = "display: none";
    }
    else {
        document.getElementById('tab-update').style = "";
    }
}

window.addEventListener('DOMContentLoaded', async () => {
    let target = document.getElementById('target');

    let headerResponse = await fetch('api/getSplitHeader/lcs');
    let headerText = await headerResponse.text();
    target.insertAdjacentHTML('afterbegin', headerText);

    // Use cookie to store current league
    let statusResponse = await fetch('api/getPlayoffStatuses/lcs/all/');
    let statusText = await statusResponse.text();
    target.insertAdjacentHTML('beforeend', statusText);

    let cumulative = document.getElementById('cumulative');
    let split = document.getElementById('split');

    cumulative.addEventListener('click', async () => {
        let league = document.querySelector("#tab-league>ul>.is-active>a").id
        statusResponse = await fetch('api/getPlayoffStatuses/' + league + '/all/');
        statusText = await statusResponse.text();

        document.getElementById('records').remove();
        cumulative.parentElement.classList.add('is-active');
        split.parentElement.classList.remove('is-active');
        target.insertAdjacentHTML('beforeend', statusText);
    });

    split.addEventListener('click', async () => {
        let league = document.querySelector("#tab-league>ul>.is-active>a").id
        statusResponse = await fetch('api/getPlayoffStatuses/' + league + '/split/');
        statusText = await statusResponse.text();

        document.getElementById('records').remove();
        split.parentElement.classList.add('is-active');
        cumulative.parentElement.classList.remove('is-active');
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

