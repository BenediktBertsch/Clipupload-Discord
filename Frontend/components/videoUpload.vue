<template>
  <div>
    <v-row
      ><v-col :cols="2" class="center" style="padding: unset"
        ><v-progress-circular
          indeterminate
          color="primary"
          small
          :width="2"
          :size="20"
        ></v-progress-circular></v-col
      ><v-col :cols="9" class="notCentered" style="padding: unset"
        >Uploading {{ getUploadIndex() }} of {{ videoCount }} videos</v-col
      ><v-col :cols="1" class="end" style="padding: unset">
        <v-btn icon @click="collapse = !collapse"
          ><v-icon v-if="collapse">mdi-chevron-up</v-icon
          ><v-icon v-else>mdi-chevron-down</v-icon></v-btn
        ></v-col
      ></v-row
    >
    <div v-if="!collapse">
      <v-row v-for="(video, i) in videos" :key="i"
        ><v-col class="center" :cols="3"
          ><img style="height: 25px" :src="video.preview" /></v-col
        ><v-col class="notCentered" style="padding: unset" :cols="8"
          ><v-tooltip bottom
            ><template v-slot:activator="{ on, attrs }">
              <span v-bind="attrs" v-on="on">
                {{ videoTitle(video.video.name) }}
              </span> </template
            ><span>{{ video.video.name }}</span></v-tooltip
          ></v-col
        ><v-col
          v-if="video.uploading"
          class="start"
          style="padding: unset"
          :cols="1"
          @mouseenter="video.hover = true"
          @mouseleave="video.hover = false"
        >
          <div v-if="!video.hover">
            <v-progress-circular
              :value="video.uploadPercentage"
              small
              :width="2"
              :size="20"
            ></v-progress-circular>
          </div>
          <div v-else>
            <v-btn @click="abort(i)" icon x-small>
              <v-icon small>mdi-close</v-icon>
            </v-btn>
          </div></v-col
        ><v-col
          v-else-if="video.finished"
          class="start"
          style="padding: unset"
          :cols="1"
          ><v-icon small> mdi-check </v-icon></v-col
        ><v-col
          v-else
          class="start"
          style="padding: unset"
          :cols="1"
          @mouseenter="video.hover = true"
          @mouseleave="video.hover = false"
          ><div v-if="!video.hover">
            <v-progress-circular
              :value="video.uploadPercentage"
              small
              :width="2"
              :size="20"
            ></v-progress-circular>
          </div>
          <div v-else>
            <v-btn @click="abort(i)" icon x-small>
              <v-icon small>mdi-close</v-icon>
            </v-btn>
          </div></v-col
        ></v-row
      >
      <v-divider></v-divider>
      <v-row
        ><v-col :cols="10"
          ><div class="start">
            <p style="margin: unset">Sharing</p>
            <v-switch
              style="transform: scale(0.75); margin: unset; padding: unset"
              hide-details
              v-model="shareUpload"
            ></v-switch></div></v-col
        ><v-col :cols="2" class="end" style="padding: unset; padding-right: 5px"
          ><v-btn @click="abortAll()" icon x-small>
            <v-icon>mdi-close</v-icon>
          </v-btn></v-col
        ></v-row
      >
    </div>
  </div>
</template>

<script>
export default {
  props: {
    videos: Array,
    uploadPercentage: Number,
    share: Boolean
  },
  data() {
    return {
      hover: false,
      videoCount: Number,
      collapse: false,
      shareUpload: this.share
    };
  },
  mounted() {
    this.videoCount = this.videos.length;
  },
  watch: {
    videos(value) {
      this.videoCount = value.length;
    },
    async shareUpload(value) {
      this.$axios.setHeader('Authorization', "Bearer " + localStorage.getItem("token"))
      await this.$axios.$patch(`${this.$config.apiURL}post?post=${value}`);
    },
  },
  methods: {
    getUploadIndex() {
      for (let index = 0; index < this.videos.length; index++) {
        if (this.videos[index].uploading) {
          return index + 1;
        }
      }
    },
    getVideoCount() {
      return this.videos.length;
    },
    abort(index) {
      if (this.videos[index].uploading) {
        this.$bus.$emit("abortUploadEvent");
      } else {
        this.videos.splice(index, 1);
      }
    },
    abortAll() {
      this.videos.forEach((video, i) => {
        if (!video.uploading && !video.finished) {
          this.videos.splice(i, 1);
        }
      });
      this.$bus.$emit("abortUploadEvent");
    },
    videoTitle(videoname) {
      if (videoname.length > 25) {
        var name = videoname.slice(0, 25);
        if (name[name.length] === " ") {
          return name + "...";
        } else {
          return name + " ...";
        }
      } else {
        return videoname;
      }
    },
  },
};
</script>

<style lang="scss" scoped>
.switch-center {
  display: flex;
  justify-content: center;
}
.center {
  height: auto;
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
}
.notCentered {
  height: auto;
  display: flex;
  align-items: center;
  width: 100%;
}
.end {
  height: auto;
  display: flex;
  justify-content: flex-end;
  align-items: center;
  width: 100%;
}
.start {
  height: auto;
  display: flex;
  justify-content: flex-start;
  align-items: center;
  width: 100%;
}
</style>
