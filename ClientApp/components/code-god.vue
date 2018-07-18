<template>
    <Form :model="formItem" :label-width="110" style="margin-top: 60px">
        <FormItem label="连接">
            <Row>
                <Col span="16">
                <Select v-model="formItem.connName">
                    <Option :value="connName" v-for="(connName,i) in connList">{{connName}}</Option>
                </Select>
                </Col>
                <Col span="8">
                <Button type="primary" style="margin-left: 8px" @click="connect">连接</Button>
                <Button type="primary">导入模型</Button>
                </Col>
            </Row>
        </FormItem>
        <FormItem label="数据表">
            <Row>
                <Col span="10">
                <Select v-model="formItem.tabName">
                    <Option :value="tabName" v-for="tabName in tabList">{{tabName}}</Option>
                </Select>
                </Col>
                <Col span="14">
                <Button type="primary" style="margin-left: 8px"  @click="genTable">生成该表</Button>
                <!--<Button type="primary">生成所有表</Button>
                <Button type="primary">刷新数据表</Button>-->
                </Col>
            </Row>
        </FormItem>
        <FormItem label="模板">
            <Row>
                <Col span="10">
                <Select v-model="formItem.templateName">
                    <Option v-for="templateName in templateList" :value="templateName">{{templateName}}</Option>
                </Select>
                </Col>
                <Col span="14">
                <Button type="primary" style="margin-left: 8px" @click="openDir('Template')">打开模板目录</Button>
                </Col>
            </Row>
        </FormItem>
        <Row>
            <Col span="12">
            <FormItem label="命名空间">
                <Input v-model="formItem.nameSpace" placeholder=""></Input>

            </FormItem>
            </Col>
            <Col span="12">
            <FormItem label="连接名">
                <Input v-model="formItem.entityConnName" placeholder=""></Input>

            </FormItem>
            </Col>
        </Row>
        <Row>
            <Col span="12">
            <FormItem label="实体基类">
                <Input v-model="formItem.baseClass" placeholder=""></Input>

            </FormItem>
            </Col>
            <Col span="12">
            <FormItem label="生成泛型实体类">
                <Checkbox v-model="formItem.renderGenEntity" ></Checkbox>
            </FormItem>
            </Col>
        </Row>
        <Row>
            <Col span="12">
            <FormItem label="输出目录">
                <Input v-model="formItem.outputPath" placeholder=""></Input>
            </FormItem>
            </Col>
            <Col span="12">
            <FormItem label="中文文件名">
                <Row>
                    <Col span="12">
                        <Checkbox v-model="formItem.useCNFileName"></Checkbox>
                    </Col>
                    <Col span="12">
                    <Button type="primary" @click="openDir(formItem.outputPath)">打开目录</Button>
                    </Col>
                </Row>

            </FormItem>
            </Col>
        </Row>
    </Form>
</template>
<script>
    export default {
        data() {
            return {
                connList: [],
                tabList: [],
                templateList:[],
                formItem: {
                    connName: '',
                    tabName: '',
                    templateName: '',
                    nameSpace: 'Models',
                    entityConnName: 'Conn',
                    baseClass: 'Entity',
                    renderGenEntity: true,
                    useCNFileName:true,
                    outputPath: "Output"
                }
            };
        },
        methods: {
            async openDir(dir) {
                let vm = this;
                try {
                    let response = await this.$http.get('/api/DataModel/openDir', { params: { dir: dir } })
                    vm.$Message.info(response.data);
                } catch (error) {
                    console.log(error)
                }
            },
            async genTable() {
                let vm = this;
                try {
                    let response = await this.$http.get('/api/DataModel/GenTable', { params: vm.formItem })
                    vm.$Message.info(response.data);
                } catch (error) {
                    console.log(error)
                }
            },
            async getTemplateList() {
                let vm = this;
                try {
                    let response = await this.$http.get('/api/DataModel/GetTemplateList')
                    vm.templateList = response.data
                } catch (error) {
                    console.log(error)
                }
            },
            async connect() {
                let vm = this;
                try {
                    let response = await this.$http.get('/api/DataModel/Connect', { params: { connName: vm.formItem.connName } })
                    vm.tabList = response.data;
                } catch (error) {
                    console.log(error)
                }
            }
        },
        async created() {
            // ES2017 async/await syntax via babel-plugin-transform-async-to-generator
            // TypeScript can also transpile async/await down to ES5

            let vm = this;
            try {
                let response = await this.$http.get('/api/DataModel/GetDatabaseList')
                vm.connList = response.data;
                await vm.getTemplateList();
            } catch (error) {
                console.log(error)
            }
            // Old promise-based approach
            //this.$http
            //    .get('/api/SampleData/WeatherForecasts')
            //    .then(response => {
            //        console.log(response.data)
            //        this.forecasts = response.data
            //    })
            //    .catch((error) => console.log(error))*/
        }
    };
</script>
