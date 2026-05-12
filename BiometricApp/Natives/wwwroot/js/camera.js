
export async function startCamera(videoId) {
    const video = document.getElementById(videoId);
    const stream = await navigator.mediaDevices.getUserMedia({ video: true });
    video.srcObject = stream;
}