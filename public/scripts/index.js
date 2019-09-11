window.addEventListener('DOMContentLoaded', async () => {
    let target = document.getElementById('target');

    let headerResponse = await fetch('api/getSplitHeader');
    let headerText = await headerResponse.text();
    target.insertAdjacentHTML('afterbegin', headerText);

    let statusResponse = await fetch('api/getPlayoffStatuses');
    let statusText = await statusResponse.text();
    target.insertAdjacentHTML('beforeend', statusText);
});