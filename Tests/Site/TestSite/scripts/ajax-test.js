class AjaxTest {

    constructor() {
        this.waitingThreads = 0;
        this.waitingThreadsMax = 5;
        this.mainLoopId = 0;
    }

    async start() {
        alert("NOW?");
        var response = await this.apiPost("readJson", { "s": "Hello world!", "i": 8, "dt": new Date() });
        document.body.innerHTML = response;
        //this.mainLoopId = window.setInterval(async () => {
        //    this.waitingThreads++;
        //    if (this.waitingThreads > this.waitingThreadsMax) {
        //        window.clearInterval(this.mainLoopId);
        //        console.info("Releasing all threads...");
        //        await this.release();
        //        console.info("Done.");
        //        return;
        //    }
        //    let threadId = this.waitingThreads;
        //    console.info("Thread " + threadId + " is waiting...");
        //    await this.waitEvent();
        //    console.info("Thread " + threadId + " exited.");
        //}, 255);

    }

    async apiPost(path, data) {
        let response = await fetch("http://localhost/" + path, {
            method: "post",
            body: JSON.stringify(data)
        });
        return await response.json();
    }

    async apiRequest(path) {
        let response = await fetch("http://localhost/" + path);
        return await response.json();
    }

    async waitEvent() {
        await this.apiRequest("waitEvent/" + Math.floor(Math.random() * Number.MAX_SAFE_INTEGER).toString());
    }

    async release() {
        await this.apiRequest("release");
    }

}