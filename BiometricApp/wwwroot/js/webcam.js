window.webcam = {

    stream: null,

    start: async function () {

        const video = document.getElementById("video");

        this.stream = await navigator.mediaDevices.getUserMedia({
            video: true,
            audio: false
        });

        video.srcObject = this.stream;
        await video.play();
    },

    stop: function () {

        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
        }

        const video = document.getElementById("video");

        if (video) {
            video.srcObject = null;
        }
    },

    capture: function () {

        const video = document.getElementById("video");

        const canvas = document.createElement("canvas");

        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;

        const ctx = canvas.getContext("2d");

        ctx.drawImage(video, 0, 0);

        return canvas.toDataURL("image/png");
    }
};