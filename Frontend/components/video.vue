<template>
  <v-card :elevation="8">
    <v-tooltip top
      ><template v-slot:activator="{ on, attrs }">
        <v-card-title v-bind="attrs" v-on="on">{{
          videoTitle(video.videoname)
        }}</v-card-title> </template
      ><span>{{ video.videoname }}</span></v-tooltip
    >
    <img
      @click="openVideo(video.id)"
      style="width: 100%; heigth: auto"
      :src="`https://drive.google.com/thumbnail?authuser=0&sz=w1000&id=${video.videoid}`"
    />
    <v-btn
      @click="copyVideoURL(video.id)"
      color="success"
      tile
      style="width: 100%"
      text
    >
      Share
    </v-btn>
    <v-divider></v-divider>
    <v-btn
      @click="deleteDialogOpen()"
      color="error"
      tile
      style="width: 100%"
      text
    >
      Delete
    </v-btn>
    <v-dialog v-model="dialog">
      <v-card>
        <v-card-title> {{ video.videoname }} </v-card-title>
        <video
          :src="`https://www.googleapis.com/drive/v3/files/${video.videoid}?key=${this.apiKey}&alt=media`"
          style="width: 100%; heigth: auto"
          controls
        ></video>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="error" text @click="dialog = false" style="width: 100%">
            Close
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-dialog v-model="deleteDialog">
      <v-card>
        <v-card-title> Delete "{{ video.videoname }}"?</v-card-title>
        <v-card-text>
          This operation will permanently delete this video.<br />
          This operation cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn
            color="error"
            text
            @click="deleteDialog = false"
            style="width: 50%"
          >
            Close
          </v-btn>
          <v-btn
            color="success"
            text
            :disabled="timer > 0"
            @click="deleteVideo(video.id)"
            style="width: 50%"
          >
            <div v-if="timer > 0">Delete ({{ timer }})</div>
            <div v-else>Delete</div>
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-card>
</template>

<script>
export default {
  props: {
    video: Object,
    userid: String,
  },
  data() {
    return {
      apiURL: null,
      apiKey: null,
      dialog: false,
      deleteDialog: false,
      timer: 0,
    };
  },
  mounted() {
    this.apiURL = this.$config.apiURL;
    this.apiKey = this.$config.apiKey;
    setInterval(() => {
      if (this.deleteDialog && this.timer > 0) {
        this.timer--;
      }
    }, 1000);
  },
  methods: {
    openVideo() {
      this.dialog = true;
    },
    deleteDialogOpen() {
      this.timer = 5;
      this.deleteDialog = true;
    },
    async deleteVideo(id) {
      var ans = await this.$axios.$delete(this.$config.apiURL + `video/${id}`, {
        headers: {
          Authorization: "Bearer " + localStorage.getItem("token"),
        }
      });
      if (ans.success) {
        this.$bus.$emit("deleteVideoEvent", id);
      }
      this.deleteDialog = false;
    },
    copyVideoURL(id) {
      var url = `${this.apiURL}video/${id}`;
      if (window.clipboardData && window.clipboardData.setData) {
        return clipboardData.setData("Text", url);
      } else if (
        document.queryCommandSupported &&
        document.queryCommandSupported("copy")
      ) {
        var textarea = document.createElement("textarea");
        textarea.textContent = url;
        textarea.style.position = "fixed";
        document.body.appendChild(textarea);
        textarea.select();
        try {
          return document.execCommand("copy");
        } catch (ex) {
          console.warn("Copy to clipboard failed.", ex);
          return false;
        } finally {
          document.body.removeChild(textarea);
        }
      }
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
    sleep(milliseconds) {
      return new Promise((resolve) => setTimeout(resolve, milliseconds));
    },
  },
};
</script>

<style>
.v-dialog {
  max-width: 500px !important;
}
</style>
