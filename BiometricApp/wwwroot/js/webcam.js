window.webcam = {

    stream: null,
    history: [], // stores states
    historyIndex: -1,
    currentImage: null,

    offsetX: 0,
    offsetY: 0,

    isDragging: false,
    startX: 0,
    startY: 0,

    lastBrightness: 0,
    lastRotation: 0,
    lastZoom: 1,

    dragInitialized: false,

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
        if (video) video.srcObject = null;
    },

    capture: function () {
        const video = document.getElementById("video");

        const canvas = document.createElement("canvas");
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;

        const ctx = canvas.getContext("2d");
        ctx.drawImage(video, 0, 0);

        const img = canvas.toDataURL("image/png");

        this.history = [img];
        this.currentImage = img;

        // ✅ RESET EVERYTHING
        this.offsetX = 0;
        this.offsetY = 0;
        this.lastBrightness = 0;
        this.lastRotation = 0;
        this.lastZoom = 1;

        this.initDrag();

        return img;
    },

    applyEdit: function (brightness, rotation, zoom) {

        if (!this.history || this.history.length === 0)
            return null;

        const baseImage = this.history[0];

        // ✅ STORE LAST VALUES
        this.lastBrightness = brightness;
        this.lastRotation = rotation;
        this.lastZoom = zoom;

        return new Promise(resolve => {

            const img = new Image();

            img.onload = () => {

                const canvas = document.createElement("canvas");
                const ctx = canvas.getContext("2d");

                canvas.width = img.width;
                canvas.height = img.height;

                ctx.clearRect(0, 0, canvas.width, canvas.height);

                ctx.save();

                ctx.translate(canvas.width / 2, canvas.height / 2);

                ctx.rotate(rotation * Math.PI / 180);
                ctx.scale(zoom, zoom);

                const b = 1 + (brightness / 100);
                ctx.filter = `brightness(${b})`;

                // ✅ LIMIT MOVEMENT (boundary control)
                const maxOffsetX = (img.width * zoom - img.width) / 2;
                const maxOffsetY = (img.height * zoom - img.height) / 2;

                this.offsetX = Math.max(-maxOffsetX, Math.min(maxOffsetX, this.offsetX));
                this.offsetY = Math.max(-maxOffsetY, Math.min(maxOffsetY, this.offsetY));

                ctx.drawImage(
                    img,
                    -img.width / 2 + this.offsetX,
                    -img.height / 2 + this.offsetY
                );

                ctx.restore();

                const edited = canvas.toDataURL("image/png");

                this.currentImage = edited;

                resolve(edited);
            };

            img.src = baseImage;
        });
    },

    undo: function () {

        if (this.history.length > 0) {
            this.currentImage = this.history[0];
        }

        this.offsetX = 0;
        this.offsetY = 0;

        return this.currentImage;
    },

    initDrag: function () {

        // ✅ PREVENT MULTIPLE BINDINGS
        if (this.dragInitialized) return;
        this.dragInitialized = true;

        const area = document.querySelector(".preview-box");

        // ---------- MOUSE ----------
        area.addEventListener("mousedown", (e) => {

            if (this.lastZoom <= 1) return;

            this.isDragging = true;
            this.startX = e.clientX - this.offsetX;
            this.startY = e.clientY - this.offsetY;
        });

        area.addEventListener("mousemove", (e) => {

            if (!this.isDragging) return;

            this.offsetX = e.clientX - this.startX;
            this.offsetY = e.clientY - this.startY;

            this.reRender();
        });

        area.addEventListener("mouseup", () => {
            this.isDragging = false;
        });

        area.addEventListener("mouseleave", () => {
            this.isDragging = false;
        });

        // ---------- TOUCH (MOBILE) ----------
        area.addEventListener("touchstart", (e) => {

            if (this.lastZoom <= 1) return;

            const touch = e.touches[0];

            this.isDragging = true;
            this.startX = touch.clientX - this.offsetX;
            this.startY = touch.clientY - this.offsetY;
        });

        area.addEventListener("touchmove", (e) => {

            if (!this.isDragging) return;

            const touch = e.touches[0];

            this.offsetX = touch.clientX - this.startX;
            this.offsetY = touch.clientY - this.startY;

            this.reRender();
        });

        area.addEventListener("touchend", () => {
            this.isDragging = false;
        });
    },

    reRender: function () {

        if (!this.currentImage) return;

        this.applyEdit(
            this.lastBrightness,
            this.lastRotation,
            this.lastZoom
        ).then(img => {

            const preview = document.querySelector(".preview-box img");
            if (preview) preview.src = img;
        });
    }
};