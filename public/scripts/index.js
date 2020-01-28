window.addEventListener('DOMContentLoaded', async () => {
    let target = document.getElementById('target');

    let headerResponse = await fetch('api/getSplitHeader/1');
    let headerText = await headerResponse.text();
    target.insertAdjacentHTML('afterbegin', headerText);

    let statusResponse = await fetch('api/getPlayoffStatuses/1');
    let statusText = await statusResponse.text();
    target.insertAdjacentHTML('beforeend', statusText);
});