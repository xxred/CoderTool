<template>
    <div style="padding:5px;">
        <Row>
            <Col span="18">
            </Col>
            <Col span="6">
            <i-button type="primary" ghost @click="save">保存</i-button>
            </Col>
        </Row>
        <Row>
            <Col span="6">
            <Tree :data="treeData"
                  @on-select-change="onSelect"></Tree>
            </Col>
            <Col span="18">
            <div style="margin: 10px;">
                <div id="editor"
                     style="width:100%;height:400px;border:1px solid grey"></div>
            </div>
            </Col>
        </Row>
    </div>

</template>
<script>
    import { editor } from "monaco-editor";
    export default {
        created() {
            let vm = this;
            vm.getTreeData();
        },
        data() {
            return {
                treeData: [],
                editor: null,
                path: ""
            };
        },
        methods: {
            createEditor() {
                let vm = this;
                vm.editor = editor.create(document.getElementById("editor"), {
                    value: [""].join("\n"),
                    language: "aspnetcorerazor",
                    theme: "vs-dark"
                });
            },
            async getTreeData() {
                let vm = this;
                try {
                    let response = await vm.$http.get("/api/File/GetTemplateList");
                    let data = response.data;
                    vm.treeData = response.data;
                } catch (error) {
                    console.log(error);
                }
            },
            async save() {
                let vm = this;
                let path = vm.path;
                let value = vm.editor.getValue();
                try {
                    let response = await vm.$http({
                        url: `/api/File/Save?path=${path}`,
                        method: 'post',
                        //headers: { 'content-type': 'application/x-www-form-urlencoded' },
                        data: { contents: value },
                        transformRequest: [function (data) {
                            // 将数据转换为表单数据
                            let ret = ''
                            for (let it in data) {
                                ret += encodeURIComponent(it) + '=' + encodeURIComponent(data[it])
                            }
                            return ret
                        }]
                    });
                    vm.$Message.success("保存成功")
                } catch (error) {
                    console.log(error);
                }
            },
            async onSelect(node) {
                if (!node || node.length < 1 || node[0].children) {
                    return;
                }
                let vm = this;
                try {
                    let path = node[0].path;
                    vm.path = path;
                    let response = await vm.$http.get(
                        `/api/File/GetFileContent?path=${path}`
                    );
                    let data = response.data;
                    vm.editor.setValue(data);
                } catch (error) {
                    console.log(error);
                }
            }
        },
        mounted() {
            let vm = this;
            vm.createEditor();
        }
    };
</script>
