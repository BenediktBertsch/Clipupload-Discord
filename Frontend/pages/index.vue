<template>
  <div
    class="center"
    @dragover.prevent
    @dragenter.prevent="ondragenter"
    @dragleave.prevent="ondragleave"
    @drop.prevent.stop="getFiles"
    id="dropzone"
  >
    <div v-if="loginStatus">
      <input
        id="file"
        type="file"
        accept="application/pdf"
        @change="getFiles"
        multiple
        hidden
      />
      <div v-if="!videos[0]">
        <v-container>
          <v-row class="center"
            ><v-icon size="250">mdi-cloud-upload</v-icon></v-row
          >
        </v-container>
      </div>
      <div v-else>
        <v-progress-circular
          v-if="loadingPosts"
          size="70"
          color="green"
          indeterminate
        />
        <div v-else class="center">
          <div
            style="
              width: 100%;
              display: flex;
              flex-wrap: wrap;
              justify-content: space-around;
              align-items: start;
              align-content: start;
              align-self: baseline;
            "
          >
            <div
              style="margin: 8px; flex-basis: 25rem"
              v-for="(video, i) in videos"
              :key="i"
            >
              <Video :video="video" />
            </div>
          </div>
        </div>
      </div>
    </div>
    <h1 v-else>Not logged in.</h1>
    <v-snackbar v-model="snackBarLoginLocal" timeout="2000">
      {{ snackBarLoginText }}
    </v-snackbar>
    <v-snackbar
      bottom
      right
      round
      v-model="snackBarUpload"
      timeout="-1"
      style="padding-bottom: unset !important"
    >
      <VideoUpload
        v-if="snackBarUpload"
        style="min-width: 300px"
        :videos="uploadVideos"
        :uploadPercentage="uploadPercentage"
        :share="share"
      />
    </v-snackbar>
  </div>
</template>

<style lang="scss" scoped>
.dropzone {
  background-color: #94949457;
}
.center {
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
}
.full {
  height: 100%;
  width: 100%;
}
::v-deep .v-snack__wrapper {
  min-width: unset !important;
}
::v-deep .v-snack__content {
  padding-bottom: unset !important;
}
</style>

<script>
import Video from "~/components/video.vue";
import VideoUpload from "~/components/videoUpload.vue";
export default {
  components: {
    Video,
    VideoUpload,
  },
  name: "Uploadmodule",
  props: {
    loginStatus: Boolean,
    snackBarLogin: Boolean,
    snackBarLoginText: String,
  },
  data() {
    return {
      snackBarLoginLocal: this.snackBarLogin,
      drop: false,
      uploadPercentage: 0,
      windowSize: {
        x: 0,
        y: 0,
      },
      videos: [],
      maxVideos: null,
      videoCount: 24,
      videoOffset: 0,
      active: false,
      loadingPosts: true,
      loadingMorePosts: false,
      snackBarUpload: false,
      share: null,
      uploadVideos: [],
      droppedFiles: [],
      uploadingVideoCount: 0,
      uploadSize: 0,
      request: new XMLHttpRequest(),
      uploadingIndex: 0,
      delCounter: 0,
    };
  },
  created() {
    this.$bus.$on("logoutEvent", (data) => {
      this.snackBarLoginLocal = data;
    });
    this.$bus.$on("deleteVideoEvent", (id) => {
      this.deleteVideo(id);
    });
    this.$bus.$on("abortUploadEvent", () => {
      this.abortUpload();
    });
  },
  beforeDestroy() {
    this.$bus.$off("deleteVideoEvent");
    this.$bus.$off("logoutEvent");
    this.$bus.$off("abortUploadEvent");
  },
  async mounted() {
    if (
      localStorage.getItem("token") !== "undefined" &&
      localStorage.getItem("token")
    ) {
      this.loadingPosts = true;
      await this.getVideos();
      await this.getShare();
      this.endLessScroll();
      this.loadingPosts = false;
    }
  },
  methods: {
    abortUpload() {
      this.request.abort();
    },
    deleteVideo(id) {
      for (let i = 0; i < this.videos.length; i++) {
        if (this.videos[i].id === id) {
          this.videos.splice(i, 1);
          this.$forceUpdate();
          return;
        }
      }
    },
    async getShare() {
      var getShare = await this.$axios.$get(`${this.$config.apiURL}post`, {
        headers: {
          Authorization: "Bearer " + localStorage.getItem("token"),
        },
      });
      this.share = getShare.post;
    },
    async getVideos() {
      var getVideos = await this.$axios.$get(
        `${this.$config.apiURL}videos?count=${this.videoCount}&offset=${this.videoOffset}`,
        {
          headers: {
            Authorization: "Bearer " + localStorage.getItem("token"),
          },
        }
      );
      this.maxVideos = getVideos.max;
      getVideos.videos.forEach((video) => {
        this.videos.push(video);
      });
      this.loadingMorePosts = false;
    },
    endLessScroll() {
      var reachedMax = false;
      window.onscroll = () => {
        let threshold =
          document.documentElement.scrollTop + window.innerHeight >=
          document.documentElement.offsetHeight * 0.9;
        if (
          threshold &&
          this.loadingMorePosts === false &&
          this.videoCount < this.maxVideos &&
          !reachedMax
        ) {
          this.loadingMorePosts = true;
          this.videoOffset += this.videoCount;
          this.videoCount = 12;
          if (!reachedMax) {
            this.getVideos();
          }
          if (this.videoCount + this.videoOffset > this.maxVideos) {
            reachedMax = true;
          }
        }
      };
    },
    ondragenter: function (e) {
      if (this.drop == false && this.loginStatus) {
        this.drop = true;
        document.getElementById("dropzone").classList.toggle("dropzone");
      }
    },
    ondragleave: function (e) {
      if (e.x == 0 && e.y == 0 && this.drop == true && this.loginStatus) {
        this.drop = false;
        document.getElementById("dropzone").classList.toggle("dropzone");
      }
    },
    async fileUpload() {
      this.delCounter = 0;
      this.snackBarUpload = true;
      for (
        let index = 0;
        index < this.uploadVideos.length + this.delCounter;
        index++
      ) {
        if (!this.uploadVideos[index - this.delCounter].finished) {
          var id;
          this.uploadingVideoCount === 0
            ? (this.uploadingVideoCount = 1)
            : this.uploadingVideoCount++;
          var data = new FormData();
          this.uploadSize = 0;
          data.append("file", this.uploadVideos[index - this.delCounter].video);
          this.uploadSize +=
            this.uploadVideos[index - this.delCounter].video.size;
          this.request = new XMLHttpRequest();
          await new Promise((resolve) => {
            this.uploadingIndex = index;
            this.uploadVideos[index - this.delCounter].uploading = true;
            this.request.onprogress = this.request.open(
              "POST",
              this.$config.apiURL + "upload"
            );
            this.request.setRequestHeader(
              "Authorization",
              "Bearer " + localStorage.getItem("token")
            );
            this.request.onreadystatechange = () => {
              if (
                this.request.readyState === XMLHttpRequest.DONE &&
                this.request.status === 200 &&
                !this.uploadVideos[index - this.delCounter].finished
              ) {
                var ans = JSON.parse(this.request.responseText);
                if (ans.success) {
                  this.uploadVideos[index - this.delCounter].uploading = false;
                  this.uploadVideos[index - this.delCounter].finished = true;
                }
                resolve();
              }
            };
            this.request.upload.addEventListener(
              "progress",
              this.progressHandler,
              false
            );
            this.request.addEventListener(
              "abort",
              () => {
                this.uploadVideos.forEach((video) => {
                  if (video.finished) finished++;
                });
                if (this.uploadVideos.length > 1) {
                  this.delCounter++;
                }
                this.uploadVideos.splice(index - this.delCounter, 1);
                resolve();
              },
              false
            );
            this.request.send(data);
          });
        }
      }
      this.snackBarUpload = false;
      this.uploadVideos = [];
    },
    progressHandler: function (event) {
      this.uploadVideos[
        this.uploadingIndex - this.delCounter
      ].uploadPercentage = Math.floor(event.loaded / (this.uploadSize / 100));
    },
    chooseFiles: function (e) {
      document.getElementById("file").click();
    },
    async getFiles(e) {
      var dropzone = document.getElementById("dropzone");
      this.drop = false;
      var files = [];
      if (e.type == "change") {
        files = e.target.files;
      } else if (this.loginStatus) {
        dropzone.classList.toggle("dropzone");
        files = e.dataTransfer.files;
      }
      for (let index = 0; index < files.length; index++) {
        var preview = URL.createObjectURL(
          await this.getVideoCover(files[index], 0)
        );
        this.uploadVideos.push({
          video: files[index],
          uploading: false,
          finished: false,
          hover: false,
          uploadPercentage: 0,
          preview,
        });
      }
      if (!this.snackBarUpload) await this.fileUpload();
    },
    getVideoCover(file, seekTo = 0.0) {
      return new Promise((resolve, reject) => {
        const videoPlayer = document.createElement("video");
        videoPlayer.setAttribute("src", URL.createObjectURL(file));
        videoPlayer.load();
        videoPlayer.addEventListener("loadedmetadata", () => {
          setTimeout(() => {
            videoPlayer.currentTime = seekTo;
          }, 200);
          videoPlayer.addEventListener("seeked", () => {
            const canvas = document.createElement("canvas");
            canvas.width = videoPlayer.videoWidth;
            canvas.height = videoPlayer.videoHeight;
            const ctx = canvas.getContext("2d");
            ctx.drawImage(videoPlayer, 0, 0, canvas.width, canvas.height);
            ctx.canvas.toBlob(
              (blob) => {
                resolve(blob);
              },
              "image/jpeg",
              1
            );
          });
        });
      });
    },
  },
};
</script>
