window.addEventListener('DOMContentLoaded', async () => {
    let target = document.getElementById('target');

    let headerResponse = await fetch('api/getSplitHeader/splitHeader/lcs');
    let headerText = await headerResponse.text();
    target.insertAdjacentHTML('afterbegin', headerText);

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

    lcs.addEventListener('click', async () => {
        lcs.parentElement.classList.add('is-active');
        lec.parentElement.classList.remove('is-active');

        let recordType = document.querySelector("#tab-update>ul>.is-active>a").id;
        document.getElementById(recordType).click();
    });

    lec.addEventListener('click', async () => {
        lec.parentElement.classList.add('is-active');
        lcs.parentElement.classList.remove('is-active');

        let recordType = document.querySelector("#tab-update>ul>.is-active>a").id;
        document.getElementById(recordType).click();
    });
});

