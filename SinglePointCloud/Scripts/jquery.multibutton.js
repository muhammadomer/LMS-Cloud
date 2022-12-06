/**
 * jQuery MultiButton v 1.7.4
 *
 * Генерация и группировка кнопок
 *
 * @autor Чечота Александр
 * @www http://uasay.com/multibutton/
 *
 * @ver 2017.05.16
 */

(function($) {

	// Основная функция инициализации
	$.buttons = function(element,options) {

		if (element.btns) return false;

		// Склеиваем опции
		options = $.extend({
			groupClass: 'multibutton',
			onChange:   false,
			onClick:    false,
			single:     true,
			needFill:   true,
			nofixed:    false,
			depend:     ',',
			textDepend: ', '
		},options);

		var $obj = $(element);

		// Класс методов
		var btns = {

			// Инициализация экземпляра
			init: function() {

				if ($obj.length) {

					// Присваиваем стиль
					$obj.addClass(options.groupClass);

					$('li',$obj).mousedown(this.mousedown);
					$('li',$obj).mouseup(this.mouseup);
					$('li',$obj).mouseout(this.mouseout);
					$('li',$obj).click(this.click);

					$obj[0].onselectstart = function() {
						return false;
					};

					// Установка начального значения
					if ($obj.attr('rel'))
						this.setValue($obj.attr('rel'));
				}
			},

			mousedown: function() {
				$(this).addClass('action');
			},

			mouseup: function() {
				$(this).removeClass('action');
			},

			// Обработка ухода мышки и отжатия
			mouseout: function() {
				$(this).removeClass('action');
			},

			// Нажатие на кнопку
			click: function() {

				var actived = $('li.active',$obj).length;

				if (options.single) {
					if ($(this).hasClass('active') && !options.needFill)
						$(this).removeClass('active');
					else {
						$('li',$obj).removeClass('active');
						$(this).addClass('active');
					}
				} else {
					if ($(this).hasClass('active')) {
						if ((actived > 1) || ((actived == 1) && !options.needFill))
							$(this).removeClass('active');
					} else
						$(this).addClass('active');
				}

				if (options.onChange && $(this).hasClass('active'))
					options.onChange($(this),$(this).attr('rel'));

				if (options.onClick)
					options.onClick($(this),btns.getValue());

				if (options.nofixed)
					$(this).removeClass('active');

				return false;
			},

			// Собираем выбранные пункты
			getValue: function(text) {

				var text    = (text) ? text : false;
				var results = [];

				$('li.active',$obj).each(function() {
					results.push((text) ? $(this).text() : $(this).attr('rel'));
				});

				return results.join((text) ? options.textDepend : options.depend);
			},

			// Установка значения
			setValue: function(value,external) {

				value      = value + '';
				var values = value.split(options.depend);

				$('li',$obj).each(function() {
						if ($.inArray($(this).attr('rel'),values) > -1) {
							$(this).addClass('active');
							if (external) {
								if (options.onChange)
									options.onChange($(this),$(this).attr('rel'));
							}
						} else
							$(this).removeClass('active');
					}
				);
			}
		};

		element.btns    = btns;
		element.options = options;

		btns.init();

		return element;
	};

	// Конструктор
	$.fn.multiButton = function(options) {

		var options = (options) ? options : {};
		var join    = options.hasOwnProperty('join') ? options.join : false;

		if (join) {
			$.buttons(this,options);
			return this;
		} else
			return this.each(function() {
				$.buttons(this,options);
			});
	};

	// Деструктор
	$.fn.multiDelete = function() {

		this.each(function() {
			if (typeof this.btns != 'undefined')
				$('li',$(this)).unbind();
			else
				console.warn('multiDelete: selector has not been initialized!');
		});

		return this;
	};

	// Получаем выбранные значения
	$.fn.getValue = function(text) {

		var text = (text) ? text : false;

		if (typeof this.btns != 'undefined')
			return this.btns.getValue(text);
		else {
			var value = '';

			this.each(function() {
				if (typeof this.btns != 'undefined') {
					newvalue = this.btns.getValue(text);
					if ((newvalue != '') && (value != ''))
						value += (text) ? this.options.textDepend : this.options.depend;
					value += newvalue;
				} else
					value = null
			});

			if (value == null)
				console.warn('getValue: selector has not been initialized!');

			return value;
		}
	};

	// Устанавливаем значение
	$.fn.setValue = function(value) {

		if (typeof this.btns != 'undefined')
			this.btns.setValue(value,true);
		else {
			var did = false;

			this.each(function() {
				if (typeof this.btns != 'undefined') {
					this.btns.setValue(value,true);
					did = true;
				}
			});

			if (!did)
				console.warn('setValue: selector has not been initialized!');
		}
	};

})(jQuery);