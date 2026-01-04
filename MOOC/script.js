// ==UserScript==
// @name         中国大学慕课AI答题助手
// @namespace    http://tampermonkey.net/
// @version      1.2
// @description  自动获取题目并使用AI回答，支持多种页面格式
// @author       camvan
// @match        *://www.icourse163.org/*
// @grant        GM_xmlhttpRequest
// @grant        GM_addStyle
// ==/UserScript==

(function () {
  "use strict";

  // ========== 配置项 ==========
  const CONFIG = {
    showDebug: true, // 是否显示调试信息（请求/响应内容）
    autoClick: true, // 是否自动点击选项
    delay: 100, // 每题之间的延迟(ms)
  };

  // API配置
  const API_CONFIG = {
    baseUrl: "https://api.example.com/v1/chat/completions", // API地址
    apiKey: "sk-your-api-key", // API密钥
    model: "gpt-3.5-turbo", // 模型名称
  };

  GM_addStyle(`
        .ai-answer-container {
            margin: 10px 0;
            padding: 12px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 8px;
            color: #fff;
            font-size: 14px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        .ai-answer-title {
            font-weight: bold;
            margin-bottom: 8px;
        }
        .ai-answer-content {
            background: rgba(255,255,255,0.15);
            padding: 10px;
            border-radius: 6px;
            line-height: 1.6;
            white-space: pre-wrap;
        }
        .ai-answer-loading::after {
            content: '...';
            animation: dots 1.5s steps(4, end) infinite;
        }
        @keyframes dots {
            0%, 20% { content: ''; }
            40% { content: '.'; }
            60% { content: '..'; }
            80%, 100% { content: '...'; }
        }
        .ai-helper-btn {
            position: fixed;
            right: 20px;
            bottom: 20px;
            z-index: 9999;
            padding: 12px 24px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #fff;
            border: none;
            border-radius: 25px;
            cursor: pointer;
            font-size: 14px;
            font-weight: bold;
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
        }
        .ai-helper-btn:hover {
            transform: translateY(-2px);
        }
        .ai-helper-btn:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }
        .ai-debug {
            margin-top: 10px;
            padding: 8px;
            background: rgba(0,0,0,0.2);
            border-radius: 4px;
            font-size: 12px;
            max-height: 200px;
            overflow-y: auto;
        }
        .ai-debug-title {
            font-weight: bold;
            margin-bottom: 4px;
            color: #ffd700;
        }
        .ai-debug pre {
            margin: 0;
            white-space: pre-wrap;
            word-break: break-all;
        }
        .ai-processed {
            /* 标记已处理的题目 */
        }
    `);

  // ========== 格式1: 中国大学慕课.html (form表单+ant-design样式) ==========
  function findQuestionsFormat1() {
    const questions = [];
    const seen = new Set();

    const formEl = document.querySelector("form");
    if (!formEl) {
      return questions;
    }

    const allDivs = formEl.querySelectorAll("div");

    allDivs.forEach(function (div) {
      const text = div.innerText || "";
      if (
        /^\d+\.(单选|多选|判断|填空|问答)/.test(text.trim()) &&
        /[A-D]\./.test(text)
      ) {
        const key = text.substring(0, 50);
        if (seen.has(key)) return;

        const matches = text.match(/\d+\.(单选|多选|判断|填空|问答)/g);
        if (matches && matches.length === 1) {
          seen.add(key);

          const lines = text.split("\n").filter(function (l) {
            return l.trim().length > 0;
          });

          let questionType = "";
          let questionTitle = "";
          let options = [];

          if (lines[0]) {
            const typeMatch = lines[0].match(/(单选|多选|判断|填空|问答)/);
            if (typeMatch) questionType = typeMatch[1];
          }

          var currentOption = "";
          for (var i = 1; i < lines.length; i++) {
            var line = lines[i].trim();

            if (/^[A-D][.\s、]?$/.test(line) || /^[A-D]$/.test(line)) {
              currentOption = line.replace(/[.\s、]/g, "") + ". ";
            } else if (/^[A-D][.\s、]/.test(line) && line.length > 3) {
              options.push(line);
              currentOption = "";
            } else if (currentOption && line.length > 0) {
              options.push(currentOption + line);
              currentOption = "";
            } else if (
              line.length > 5 &&
              !questionTitle &&
              !/^[A-D]/.test(line)
            ) {
              questionTitle = line;
            }
          }

          if (questionTitle) {
            questions.push({
              element: div,
              questionType: questionType,
              questionTitle: questionTitle,
              options: options,
              format: 1,
            });
          }
        }
      }
    });

    return questions;
  }

  // ========== 格式2: 测试.html (u-questionItem格式) ==========
  function findQuestionsFormat2() {
    const questions = [];

    // 查找所有题目容器
    const questionItems = document.querySelectorAll(
      ".u-questionItem, .m-choiceQuestion",
    );

    questionItems.forEach(function (item) {
      // 避免重复处理
      if (item.classList.contains("ai-processed")) return;

      // 获取题目类型
      let questionType = "";
      const typeEl = item.querySelector(".qaCate span");
      if (typeEl) {
        questionType = typeEl.innerText.trim();
      }

      // 获取题目内容
      let questionTitle = "";
      const titleEl = item.querySelector(".f-richEditorText.j-richTxt");
      if (titleEl) {
        questionTitle = titleEl.innerText.trim();
      }

      // 获取选项
      const options = [];
      const optionItems = item.querySelectorAll("ul.choices li");
      optionItems.forEach(function (li) {
        const posEl = li.querySelector(".optionPos");
        const cntEl = li.querySelector(".optionCnt");
        if (posEl && cntEl) {
          const letter = posEl.innerText.trim();
          const content = cntEl.innerText.trim();
          options.push(letter + " " + content);
        }
      });

      if (questionTitle) {
        questions.push({
          element: item,
          questionType: questionType,
          questionTitle: questionTitle,
          options: options,
          format: 2,
        });
      }
    });

    return questions;
  }

  // ========== 主查找函数 ==========
  function findAllQuestions() {
    // 优先尝试格式2 (测试.html - u-questionItem)
    let questions = findQuestionsFormat2();
    if (questions.length > 0) {
      console.log("使用格式2找到题目:", questions.length);
      return questions;
    }

    // 尝试格式1 (中国大学慕课.html - form表单)
    questions = findQuestionsFormat1();
    if (questions.length > 0) {
      console.log("使用格式1找到题目:", questions.length);
      return questions;
    }

    console.log("未找到题目");
    return [];
  }

  function callAI(prompt) {
    return new Promise(function (resolve, reject) {
      console.log("发送API请求...");

      var requestBody = JSON.stringify({
        model: API_CONFIG.model,
        messages: [
          {
            role: "system",
            content:
              '你是答题助手。对于所有选择题（包括判断题）直接回答选项字母如"A"或"B"；填空题和问答题简洁作答。判断题中A代表正确/对，B代表错误/错。',
          },
          {
            role: "user",
            content: prompt,
          },
        ],
      });

      GM_xmlhttpRequest({
        method: "POST",
        url: API_CONFIG.baseUrl,
        headers: {
          "Content-Type": "application/json",
          Authorization: "Bearer " + API_CONFIG.apiKey,
        },
        data: requestBody,
        onload: function (response) {
          console.log("API响应状态:", response.status);
          console.log("API响应内容:", response.responseText);

          var debugInfo = {
            request: requestBody,
            response: response.responseText,
            status: response.status,
          };

          try {
            var data = JSON.parse(response.responseText);

            if (data.choices && data.choices[0] && data.choices[0].message) {
              resolve({
                answer: data.choices[0].message.content,
                debug: debugInfo,
              });
            } else if (data.response) {
              resolve({ answer: data.response, debug: debugInfo });
            } else if (data.content) {
              resolve({ answer: data.content, debug: debugInfo });
            } else if (data.error) {
              reject({
                error:
                  "API错误: " +
                  (data.error.message || JSON.stringify(data.error)),
                debug: debugInfo,
              });
            } else {
              reject({
                error: "未知响应格式",
                debug: debugInfo,
              });
            }
          } catch (e) {
            reject({
              error: "JSON解析失败: " + e.message,
              debug: debugInfo,
            });
          }
        },
        onerror: function (error) {
          console.log("请求错误:", error);
          reject({
            error: "网络请求失败",
            debug: { request: requestBody, response: "网络错误", status: 0 },
          });
        },
        ontimeout: function () {
          reject({
            error: "请求超时",
            debug: { request: requestBody, response: "超时", status: 0 },
          });
        },
      });
    });
  }

  function buildPrompt(question) {
    var prompt = "题目类型: " + question.questionType + "\n";
    prompt += "题目: " + question.questionTitle + "\n";
    if (question.options.length > 0) {
      prompt += "选项:\n" + question.options.join("\n") + "\n";
    }
    prompt += "\n请直接给出答案:";
    return prompt;
  }

  function showAnswer(element, answer, isLoading, debugInfo) {
    var existing = element.querySelector(".ai-answer-container");
    if (existing) existing.remove();

    var container = document.createElement("div");
    container.className = "ai-answer-container";

    if (isLoading) {
      container.innerHTML =
        '<div class="ai-answer-title">AI答题助手</div>' +
        '<div class="ai-answer-content ai-answer-loading">正在思考中</div>';
    } else {
      var debugHtml = "";
      if (CONFIG.showDebug && debugInfo) {
        debugHtml =
          '<div class="ai-debug">' +
          '<div class="ai-debug-title">请求内容:</div>' +
          "<pre>" +
          escapeHtml(debugInfo.request) +
          "</pre>" +
          '<div class="ai-debug-title" style="margin-top:8px;">返回内容 (状态:' +
          debugInfo.status +
          "):</div>" +
          "<pre>" +
          escapeHtml(debugInfo.response) +
          "</pre>" +
          "</div>";
      }
      container.innerHTML =
        '<div class="ai-answer-title">AI答案</div>' +
        '<div class="ai-answer-content">' +
        escapeHtml(answer) +
        "</div>" +
        debugHtml;
    }

    element.insertBefore(container, element.firstChild);
    return container;
  }

  function escapeHtml(str) {
    if (!str) return "";
    return String(str)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function autoClickOption(element, answer, questionType, format) {
    if (!CONFIG.autoClick) return;

    var answerText = answer.trim().toUpperCase();
    var optionLetters = answerText.match(/[A-D]/g);

    if (!optionLetters || optionLetters.length === 0) {
      console.log("未能从答案中提取选项字母:", answer);
      return;
    }

    optionLetters = [...new Set(optionLetters)];
    console.log("提取到选项字母:", optionLetters, "格式:", format);

    optionLetters.forEach(function (letter) {
      var clicked = false;

      if (format === 2) {
        // 格式2: 测试.html - 通过optionPos查找
        var optionPosEls = element.querySelectorAll(".optionPos");
        optionPosEls.forEach(function (posEl) {
          if (clicked) return;
          var text = (posEl.innerText || "").trim().toUpperCase();
          if (text === letter + "." || text === letter) {
            var li = posEl.closest("li");
            if (li) {
              var input = li.querySelector(
                'input[type="radio"], input[type="checkbox"]',
              );
              if (input) {
                console.log("格式2点击:", letter);
                input.click();
                clicked = true;
              }
            }
          }
        });
      } else {
        // 格式1: 中国大学慕课.html - 通过optionIndex查找
        var optionIndexSpans = element.querySelectorAll(
          'span[class*="optionIndex"]',
        );
        optionIndexSpans.forEach(function (span) {
          if (clicked) return;
          var text = (span.innerText || "").trim().toUpperCase();
          if (text === letter + "." || text === letter) {
            var container = span.closest('div[class*="option"]');
            if (container) {
              var input = container.querySelector(
                'input[type="radio"], input[type="checkbox"]',
              );
              if (input) {
                console.log("格式1点击:", letter);
                input.click();
                clicked = true;
              }
            }
          }
        });
      }

      // 备用方案: 按索引点击
      if (!clicked) {
        var index = letter.charCodeAt(0) - 65;
        var allInputs = element.querySelectorAll(
          'input[type="radio"], input[type="checkbox"]',
        );
        if (allInputs.length > index) {
          console.log("按索引点击:", letter, "索引:", index);
          allInputs[index].click();
          clicked = true;
        }
      }

      if (!clicked) {
        console.log("未能找到选项:", letter);
      }
    });
  }

  function processQuestion(question) {
    if (question.element.classList.contains("ai-processed")) {
      return Promise.resolve();
    }
    question.element.classList.add("ai-processed");

    showAnswer(question.element, "", true, null);

    var prompt = buildPrompt(question);
    console.log("题目prompt:", prompt);

    return callAI(prompt)
      .then(function (result) {
        showAnswer(question.element, result.answer, false, result.debug);
        autoClickOption(
          question.element,
          result.answer,
          question.questionType,
          question.format,
        );
      })
      .catch(function (result) {
        var errorMsg = result.error || result;
        var debugInfo = result.debug || null;
        showAnswer(question.element, "错误: " + errorMsg, false, debugInfo);
      });
  }

  function processAllQuestions() {
    var questions = findAllQuestions();

    if (questions.length === 0) {
      alert("未找到题目，请确保页面已完全加载后重试");
      return Promise.resolve();
    }

    console.log("开始处理 " + questions.length + " 道题目");

    var index = 0;
    function processNext() {
      if (index >= questions.length) {
        return Promise.resolve();
      }

      var q = questions[index];
      index++;

      return processQuestion(q)
        .then(function () {
          return new Promise(function (r) {
            setTimeout(r, CONFIG.delay);
          });
        })
        .then(processNext);
    }

    return processNext();
  }

  function createFloatingButton() {
    var btn = document.createElement("button");
    btn.className = "ai-helper-btn";
    btn.textContent = "AI答题";
    btn.onclick = function () {
      btn.disabled = true;
      btn.textContent = "处理中...";
      processAllQuestions()
        .then(function () {
          btn.disabled = false;
          btn.textContent = "AI答题";
        })
        .catch(function (e) {
          console.error(e);
          btn.disabled = false;
          btn.textContent = "AI答题";
        });
    };
    document.body.appendChild(btn);
  }

  if (document.readyState === "complete") {
    setTimeout(createFloatingButton, 1000);
  } else {
    window.addEventListener("load", function () {
      setTimeout(createFloatingButton, 1000);
    });
  }
})();
