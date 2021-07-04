window.addEventListener('DOMContentLoaded', async () => {
    let target = document.getElementById('target');

    let headerResponse = await fetch('api/getSplitHeader/splitHeader');
    let headerText = await headerResponse.text();
    target.insertAdjacentHTML('afterbegin', headerText);

    let statusResponse = await fetch('api/getPlayoffStatuses/all');
    let statusText = await statusResponse.text();
    target.insertAdjacentHTML('beforeend', statusText);

    let cumulative = document.getElementById('cumulative');
    let split = document.getElementById('split');

    cumulative.addEventListener('click', async () => {
        statusResponse = await fetch('api/getPlayoffStatuses/all');
        statusText = await statusResponse.text();
        document.getElementById('records').remove();
        cumulative.parentElement.classList.add('is-active');
        split.parentElement.classList.remove('is-active');
        target.insertAdjacentHTML('beforeend', statusText);
    })

    split.addEventListener('click', async () => {
        statusResponse = await fetch('api/getPlayoffStatuses/split');
        statusText = await statusResponse.text();
        document.getElementById('records').remove();
        split.parentElement.classList.add('is-active');
        cumulative.parentElement.classList.remove('is-active');
        target.insertAdjacentHTML('beforeend', statusText);
    })
});

