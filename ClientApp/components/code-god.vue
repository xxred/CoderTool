<template>
    <Form :model="formItem" :label-width="110" style="margin-top: 60px">
        <FormItem label="连接">
            <Row>
                <Col span="16">
                    <Select v-model="formItem.select">
                <Option value="beijing">New York</Option>
                <Option value="shanghai">London</Option>
                <Option value="shenzhen">Sydney</Option>
            </Select>
                </Col>
                <Col span="8">
                    <Button type="primary" style="margin-left: 8px">连接</Button>
                    <Button type="primary">导入模型</Button>
                </Col>
            </Row>
        </FormItem>
     <FormItem label="数据表">
            <Row>
                <Col span="10">
                    <Select v-model="formItem.select">
                <Option value="beijing">New York</Option>
                <Option value="shanghai">London</Option>
                <Option value="shenzhen">Sydney</Option>
            </Select>
                </Col>
                <Col span="14">
                    <Button type="primary" style="margin-left: 8px">生成该表</Button>
                    <Button type="primary">生成所有表</Button>
                    <Button type="primary">刷新数据表</Button>
                </Col>
            </Row>
        </FormItem>
             <FormItem label="模板">
            <Row>
                <Col span="10">
                    <Select v-model="formItem.select">
                <Option value="beijing">New York</Option>
                <Option value="shanghai">London</Option>
                <Option value="shenzhen">Sydney</Option>
            </Select>
                </Col>
                <Col span="14">
                    <Button type="primary" style="margin-left: 8px">释放内部模板</Button>
                </Col>
            </Row>
        </FormItem>
      <Row>
                <Col span="12">
     <FormItem label="命名空间">
                    <Input v-model="formItem.input" placeholder="Enter something..."></Input>
      
        </FormItem>
                </Col>
                 <Col span="12">
     <FormItem label="连接名">
                    <Input v-model="formItem.input" placeholder="Enter something..."></Input>
      
        </FormItem>
                </Col>
            </Row>
         <Row>
                <Col span="12">
     <FormItem label="实体基类">
                    <Input v-model="formItem.input" placeholder="Enter something..."></Input>
      
        </FormItem>
                </Col>
                 <Col span="12">
     <FormItem label="生成泛型实体类">
                       <CheckboxGroup v-model="formItem.checkbox">
                <Checkbox label="Movie"></Checkbox>
            </CheckboxGroup>
      
        </FormItem>
                </Col>
            </Row>
              <Row>
                <Col span="12">
     <FormItem label="输出目录">
                    <Input v-model="formItem.input" placeholder="Enter something..."></Input>
      
        </FormItem>
                </Col>
                 <Col span="12">
     <FormItem label="中文文件名">
         <Row>
             <Col span="12">
               <CheckboxGroup v-model="formItem.checkbox">
                <Checkbox label="Movie"></Checkbox>
            </CheckboxGroup>
             </Col>
              <Col span="12">
                    <Button type="primary">打开目录</Button>
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
      formItem: {
        input: "",
        select: "beijing",
        radio: "male",
        checkbox: [],
        switch: true,
        date: "",
        time: "",
        slider: [20, 50],
        textarea: ""
      }
    };
  },
    async created() {
        // ES2017 async/await syntax via babel-plugin-transform-async-to-generator
        // TypeScript can also transpile async/await down to ES5

      let vm = this;
        try {
            let response = await this.$http.get('/api/DataModel/GetDatabaseList')
            console.log(response.data);alert(response.data)
            this.forecasts = response.data;
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
